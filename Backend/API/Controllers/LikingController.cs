using Application.Dtos.Contents;
using Application.Dtos.Comments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Domain.Common;
using Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Application.Utilities;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LikingController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<LikingController> logger;

    public LikingController(EndlessContext context, ILogger<LikingController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentDto>> LikeContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var content = await context.Contents
            .Select(content => new
            {
                c = content,
                cResponse = new ContentDto(
                    content.Id, content.ChannelId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count,
                    content.Likers.Count, content.Comments.Count, content.DisLikers.Count,
                    content.ViewsCount)
            })
            .FirstOrDefaultAsync(content => content.c.Id == ContentId);

        if (currentUser is null)
            return NotFound("User not found");
        if (content is null || content.c is null)
            return NotFound("Content not found");

        LikedContent likedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            LikedDate = DateTime.UtcNow
        };

        context.LikedContents.Add(likedContent);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} liked content {ContentId}",
            currentUserId, ContentId);

        return Created($"api/liking/user/{likedContent.UserId}/content/{likedContent.ContentId}",
            content.cResponse);
    }

    [HttpGet("user/{UserId}/content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetLikedContents(Guid UserId, Guid ContentId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("current/contents")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserLikedContents()
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReLikeContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        Content? content = await context.Contents
            .Include(content => content.Creator)
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        LikedContent? likedContent = await context.LikedContents
            .FirstOrDefaultAsync(likedContent =>
                likedContent.ContentId == ContentId &&
                likedContent.UserId == currentUserId);

        if (content is null)
            return NotFound("Content not found");
        if (likedContent is null)
            return NotFound("Like dont placed");

        context.LikedContents.Remove(likedContent);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re liked content {ContentId}",
            currentUserId, ContentId);

        return NoContent();
    }

    [HttpPost("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentDto>> LikeComment(Guid CommentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var comment = await context.Comments
            .Select(comment => new
            {
                c = comment,
                cResponse = new CommentDto(
                    comment.Id,
                    comment.Text,
                    comment.PublicatedDate,
                    comment.Likers.Count,
                    comment.DisLikers.Count,
                    comment.ViewsCount)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(comment => comment.c.Id == CommentId);

        if (currentUser is null)
            return NotFound("User not found");
        if (comment is null || comment.c is null)
            return NotFound("Comment not found");

        LikedComment likedComment = new()
        {
            UserId = currentUserId,
            CommentId = CommentId,
            LikedDate = DateTime.UtcNow
        };

        context.LikedComments.Add(likedComment);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} liked comment {CommentId}",
            currentUserId, CommentId);

        return Created($"api/liking/user/{likedComment.UserId}/comment/{likedComment.CommentId}",
            comment.cResponse);
    }

    [HttpGet("user/{UserId}/comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetLikedComments(Guid UserId, Guid CommentId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("current/comments")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserLikedComments()
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReLikeComment(Guid CommentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        Comment? comment = await context.Comments
            .FirstOrDefaultAsync(comment => comment.Id == CommentId);

        LikedComment? likedComment = await context.LikedComments
            .FirstOrDefaultAsync(likedComment =>
                likedComment.UserId == currentUserId &&
                likedComment.CommentId == CommentId);

        if (comment is null)
            return NotFound("Comment not found");
        if (likedComment is null)
            return NotFound("Like dont placed");

        context.LikedComments.Remove(likedComment);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re liked comment {CommentId}",
            currentUserId, CommentId);

        return NoContent();
    }
}