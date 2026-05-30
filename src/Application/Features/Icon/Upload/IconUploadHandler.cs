using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Repositories;
using Application.Features.Rows.Contents;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Domain.Common.Enums;
using Domain.Entities;
using MediatR;


namespace Application.Features.Icon.Upload;

public class IconUploadHandler : IRequestHandler<IconUploadMessage, Result<Null>>
{
    private readonly IServiceScopeFactory factory;
    private readonly ILogger<IconUploadHandler> logger;
    private readonly IR2Service r2Service;
    
    public IconUploadHandler(IR2Service r2Service, ILogger<IconUploadHandler> logger, IServiceScopeFactory factory)
    {
        this.r2Service = r2Service;
        this.factory = factory;
        this.logger = logger;
    }

    public async Task<Result<Null>> Handle(IconUploadMessage message, CancellationToken token)
    {
        PhotoVariants iconVariants = await r2Service.SaveIconVariants(
            message.PhotoPath, message.Slug, message.Type, token);

        await using var scope = factory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        if (message.Type == IconType.User)
        {
            UserMeta? meta = await context.UserMetas
                .Include(u => u.User)
                .FirstOrDefaultAsync(u => u
                    .UserId == message.Id, token);

            if (meta == null)
            {
                logger.LogError("user not found");

                return Result<Null>.Failure(500, "user not found");
            }

            meta.SetPhoto(
                iconVariants.BaseUrl, iconVariants.Small, iconVariants.Medium, iconVariants.Large);
            await meta.SetAverageColor(
                iconVariants.Small, token);

            await context.SaveChangesAsync();

            await scope.ServiceProvider.GetRequiredService<IUserRepository>()
                .CreateSearchIndex(meta.User!, meta, token);
        }
        else if (message.Type == IconType.Channel)
        {
            ChannelMeta? meta = await context.ChannelMetas
                .Include(c => c.Channel)
                .FirstOrDefaultAsync(c => c
                    .ChannelId == message.Id, token);

            if (meta == null)
            {
                logger.LogError("channel not found");
                
                return Result<Null>.Failure(500, "channel not found");
            }

            meta.SetPhoto(
                iconVariants.BaseUrl, iconVariants.Small, iconVariants.Medium, iconVariants.Large);
            await meta.SetAverageColor(
                iconVariants.Small, token);

            await context.SaveChangesAsync();

            await scope.ServiceProvider.GetRequiredService<IChannelRepository>()
                .CreateSearchIndex(meta.Channel!, meta, token);
        }

        File.Delete(message.PhotoPath);

        return Result<Null>.Success(200, new Null());
    }
}