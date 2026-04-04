using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Managers;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DisLikingController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<DisLikingController> logger;

    public DisLikingController(EndlessContext context, ILogger<DisLikingController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentResponseDto>> DisLikeContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        bool hasUser = await context.Users.AsNoTracking().AnyAsync(user => user.Id == currentUserId);
        var content = await context.Contents.Select(content => new
        {
            c = content,
            cResponse = new ContentResponseDto(
            content.Id, content.DomainId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType.ToString(),
            content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
            content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count, content.Likers.Count,
            content.Comments.Count, content.DisLikers.Count, content.ViewsCount)
        })
            .FirstOrDefaultAsync(content => content.c.Id == ContentId);

        if (!hasUser)
            return NotFound("User not found");
        if (content is null || content.c is null)
            return NotFound("Content not found");

        DisLikedContent DisLikedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            DisLikedDate = DateTime.UtcNow
        };

        context.DisLikedContents.Add(DisLikedContent);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} dis liked content {ContentId}",
            currentUserId, ContentId);

        return Created($"api/dizliking/user/{currentUserId}/content/{ContentId}",
            content.cResponse);
    }

    [HttpGet("user/{UserId}/content/{ContentId}")]
    public async Task<ActionResult> GetDisLikedContent(Guid UserId, Guid ContentId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("current/contents")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserDisLikedContents()
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReDizLikeContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DisLikedContent? DisLikedContent = await context.DisLikedContents
            .FirstOrDefaultAsync(DisLikedContent =>
                DisLikedContent.ContentId == ContentId &&
                DisLikedContent.UserId == currentUserId);

        if (DisLikedContent is null)
            return NotFound("Like dont placed");

        context.DisLikedContents.Remove(DisLikedContent);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re dis liked content {ContentId}",
            currentUserId, ContentId);

        return NoContent();
    }

    [HttpPost("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentResponseDto>> DizLikeComment(Guid CommentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        bool hasUser = await context.Users.AsNoTracking().AnyAsync(user => user.Id == currentUserId);
        var comment = await context.Comments
            .Select(comment => new
            {
                c = comment,
                cResponse = new CommentResponseDto(
                    comment.Id,
                    comment.Text,
                    comment.PublicatedDate,
                    comment.Likers.Count,
                    comment.DisLikers.Count,
                    comment.ViewsCount)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(comment => comment.c.Id == CommentId);

        if (!hasUser)
            return NotFound("User not found");
        if (comment is null || comment.c is null)
            return NotFound("Comment not found");

        DisLikedComment DisLikedComment = new()
        {
            UserId = currentUserId,
            CommentId = CommentId,
            DisLikedDate = DateTime.UtcNow
        };

        context.DisLikedComments.Add(DisLikedComment);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} diz liked comment {CommentId}",
            currentUserId, CommentId);

        return Created($"api/dizliking/user/{currentUserId}/comment/{CommentId}", comment.cResponse);
    }

    [HttpGet("user/{UserId}/comment/{CommentId}")]
    public async Task<ActionResult> GetDisLikedComment(Guid UserId, Guid CommentId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("current/comments")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserDisLikedComments()
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReDizLikeComment(Guid CommentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DisLikedComment? DisLikedComment = await context.DisLikedComments
            .FirstOrDefaultAsync(DisLikedComment =>
                DisLikedComment.CommentId == CommentId &&
                DisLikedComment.UserId == currentUserId);

        if (DisLikedComment is null)
            return NotFound("DizLike dont placed");

        context.DisLikedComments.Remove(DisLikedComment);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re diz liked comment {CommentId}",
            currentUserId, CommentId);

        return NoContent();
    }
}