using Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Db;
using Domain.Entities;
using MediatR;

namespace Application.Features.Contents.Delete;

public class ContentDeleteHandler : IRequestHandler<ContentDeleteCommand, Result<Null>>
{
    private readonly ILogger<ContentDeleteHandler> logger;
    private readonly IContentRepository contentRepository;
    private readonly IAppDbContext context;
    public ContentDeleteHandler(IAppDbContext context, IContentRepository contentRepository, ILogger<ContentDeleteHandler> logger)
    {
        this.contentRepository = contentRepository;
        this.context = context;
        this.logger = logger;
    }

    public async Task<Result<Null>> Handle(ContentDeleteCommand cmd, CancellationToken cancellationToken)
    {
        User? user = await context.Users.FindAsync(
            cmd.UserId, cancellationToken);

        if (user is null)
            return Result<Null>.Failure(404, "User not found");

        if (user.Contents.Any(c => c.Id != cmd.ContentId))
        {
            logger.LogWarning("User {UserId} tried to create content without permission",
                cmd.UserId);
            return Result<Null>.Failure(403, "You doesn't owner the Content");
        }

        var content = await context.Contents
            .Where(content => content.Id == cmd.ContentId)
            .Select(content => new
            {
                content,
                videoPath = content.VideoMeta.VideoUrl,
                previewPath = content.VideoMeta.Image.BaseUrl
            })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (content == null)
            return Result<Null>.Failure(404, "Content not found");

        // for local storage

        if (!string.IsNullOrEmpty(content.videoPath))
        {
            string? directoryName = Path.GetDirectoryName(content.videoPath);

            if (directoryName != null)
                Directory.Delete(directoryName, true);
        }

        if (!string.IsNullOrEmpty(content.previewPath) && Directory.Exists(content.previewPath))
            Directory.Delete(content.previewPath, true);

        context.Contents.Remove(content.content);

        await context.SaveChangesAsync();

        await contentRepository.DeleteSearchIndex(
            cmd.ContentId, cancellationToken);

        logger.LogWarning("Deleted content {ContentId} for user {UserId}",
            cmd.ContentId, cmd.UserId);

        return Result<Null>.Success(204, new Null());
    }
}