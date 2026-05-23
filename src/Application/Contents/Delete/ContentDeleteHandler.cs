using Domain.Common.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Domain.Common.Interfaces.Db;
using Domain.Entities;
using MediatR;

namespace Application.Contents.Delete;

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

        Content? content = await context.Contents
            .Include(c => c.VideoMeta)
            .FirstOrDefaultAsync(c => c.Id == cmd.ContentId, cancellationToken: cancellationToken);

        if (content == null)
            return Result<Null>.Failure(404, "Content not found");

        // for local storage
        string path = content.VideoMeta.VideoUrl;

        if (!string.IsNullOrEmpty(path))
        {
            string? directoryName = Path.GetDirectoryName(path);

            if (directoryName != null)
                Directory.Delete(directoryName, true);
        }

        path = content.VideoMeta.BaseUrl;

        if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }

        context.Contents.Remove(content);

        await context.SaveChangesAsync();

        await contentRepository.DeleteSearchIndex(
            cmd.ContentId, cancellationToken);

        logger.LogWarning("Deleted content {ContentId} for user {UserId}",
            cmd.ContentId, cmd.UserId);

        return Result<Null>.Success(204, new Null());
    }
}