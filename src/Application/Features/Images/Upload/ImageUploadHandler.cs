using Microsoft.Extensions.DependencyInjection;
using Application.Features.Rows.Channels;
using Application.Features.Rows.Users;
using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Application.Features.Rows;
using Domain.Common.Enums;
using System.Text.Json;
using Domain.Entities;
using MediatR;


namespace Application.Features.Images.Upload;

public class ImageUploadHandler : IRequestHandler<ImageUploadMessage, Result<Null>>
{
    private readonly ILogger<ImageUploadHandler> logger;
    private readonly SearchIndexUpsertPublisher publisher;
    private readonly IServiceScopeFactory factory;
    private readonly IImageAnalyzer imageAnalyzer;
    private readonly IStorage Storage;

    public ImageUploadHandler(IStorage Storage, ILogger<ImageUploadHandler> logger, IImageAnalyzer imageAnalyzer, SearchIndexUpsertPublisher publisher, IServiceScopeFactory factory)
    {
        this.imageAnalyzer = imageAnalyzer;
        this.publisher = publisher;
        this.Storage = Storage;
        this.factory = factory;
        this.logger = logger;
    }

    public async Task<Result<Null>> Handle(ImageUploadMessage message, CancellationToken token)
    {
        ImageVariantsDto iconVariants = await Storage.SaveImageVariants(
            message.PhotoPath, message.Slug, [(256, 256), (128, 128), (64, 64)],
            85, message.Owner, message.Type, token);

        await using var scope = factory.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        if (message.Owner == ImageOwner.User)
        {
            UserMeta? meta = await context.UserMetas
                .Include(u => u.User)
                .Include(u => u.Image)
                .FirstOrDefaultAsync(u => u
                    .UserId == message.Id, token);

            if (meta == null)
            {
                logger.LogError("user not found");

                return Result<Null>.Failure(500, "user not found");
            }

            await UploadUserIcon(context, meta, iconVariants, token);

            await publisher.Publish(
                new SearchIndexUpsertMessage(nameof(User),
                    JsonSerializer.Serialize(new UserSearchIndex(meta.User!, meta, meta.Image))), token);
        }
        else if (message.Owner == ImageOwner.Channel)
        {
            ChannelMeta? meta = await context.ChannelMetas
                .Include(c => c.Channel)
                .Include(c => c.Image)
                .FirstOrDefaultAsync(c => c
                    .ChannelId == message.Id, token);

            if (meta == null)
            {
                logger.LogError("channel not found");

                return Result<Null>.Failure(500, "channel not found");
            }

            await UploadChannelIcon(context, meta, iconVariants, token);

            // publish to upserting search index
            await publisher.Publish(
                new SearchIndexUpsertMessage(nameof(Channel),
                    JsonSerializer.Serialize(new ChannelSearchIndex(meta.Channel!, meta, meta.Image))), token);
        }

        File.Delete(message.PhotoPath);

        logger.LogInformation("icon upload complete: process succeed");

        return Result<Null>.Success(200, new Null());
    }

    private async Task UploadUserIcon(IAppDbContext context, UserMeta meta, ImageVariantsDto iconVariants, CancellationToken token)
    {
        // delete old data

        Image image = new() { BaseUrl = "/storage/images/user" };

        if (Directory.Exists(image.BaseUrl))
            Directory.Delete(image.BaseUrl, true);

        await imageAnalyzer.SetImageVariants(
            image, iconVariants, token);

        meta.SetImage(image);

        /* old
        if (Directory.Exists(meta.IconBase))
            Directory.Delete(meta.IconBase, true);

        meta.SetPhoto(
            iconVariants.BaseUrl, iconVariants.Small, iconVariants.Medium, iconVariants.Large);

        await imageAnalyzer.SetAverageColor(
            Path.Combine(iconVariants.BaseUrl, iconVariants.Small), meta.SetColor, token);*/

        await context.SaveChangesAsync();
    }

    private async Task UploadChannelIcon(IAppDbContext context, ChannelMeta meta, ImageVariantsDto iconVariants, CancellationToken token)
    {
        // delete old data

        Image image = new() { BaseUrl = "/storage/images/channels" };

        if (Directory.Exists(image.BaseUrl))
            Directory.Delete(image.BaseUrl, true);

        await imageAnalyzer.SetImageVariants(
            image, iconVariants, token);

        meta.SetImage(image);

        /* old 
        if (Directory.Exists(meta.IconBase))
            Directory.Delete(meta.IconBase, true);

        meta.SetPhoto(
            iconVariants.BaseUrl, iconVariants.Small, iconVariants.Medium, iconVariants.Large);
        await imageAnalyzer.SetAverageColor(
            Path.Combine(iconVariants.BaseUrl, iconVariants.Small), meta.SetColor, token);
        */

        await context.SaveChangesAsync();
    }
}