using Domain.Entities;
using Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Domain.Interfaces.Repositories;

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
            .FirstOrDefaultAsync(c => c.Id == cmd.ContentId, cancellationToken: cancellationToken);

        if (content == null)
            return Result<Null>.Failure(404, "Content not found");

        context.Contents.Remove(content);

        await context.SaveChangesAsync();

        await contentRepository.DeleteSearchIndex(
            cmd.ContentId, cancellationToken);

        logger.LogWarning("Deleted content {ContentId} for user {UserId}",
            cmd.ContentId, cmd.UserId);

        return Result<Null>.Success(204, new Null());
    }
}