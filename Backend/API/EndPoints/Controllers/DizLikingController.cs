using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Dtos;
using System.Runtime.CompilerServices;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DizLikingController : ControllerBase
{
    private readonly EndlessContext context;

    public DizLikingController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentResponseDto>> DizLikeContent(Guid ContentId)
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
            content.Comments.Count, content.DizLikers.Count, content.ViewsCount)
        })
            .FirstOrDefaultAsync(content => content.c.Id == ContentId);

        if (!hasUser)
            return NotFound("User not found");
        if (content is null || content.c is null)
            return NotFound("Content not found");

        DizLikedContent dizLikedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            DizLikedDate = DateTime.UtcNow
        };

        context.DizLikedContents.Add(dizLikedContent);

        await context.SaveChangesAsync();

        return Created($"api/dizliking/user/{currentUserId}/content/{ContentId}",
            content.cResponse);
    }

    [HttpGet("user/{UserId}/content/{ContentId}")]
    public async Task<ActionResult> GetDizLikedContent(Guid UserId, Guid ContentId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("current/contents")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserDizLikedContents()
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReDizLikeContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DizLikedContent? dizLikedContent = await context.DizLikedContents
            .FirstOrDefaultAsync(dizLikedContent =>
                dizLikedContent.ContentId == ContentId &&
                dizLikedContent.UserId == currentUserId);

        if (dizLikedContent is null)
            return NotFound("Like dont placed");

        context.DizLikedContents.Remove(dizLikedContent);

        await context.SaveChangesAsync();

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
                    comment.DizLikers.Count,
                    comment.ViewsCount)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(comment => comment.c.Id == CommentId);

        if (!hasUser)
            return NotFound("User not found");
        if (comment is null || comment.c is null)
            return NotFound("Comment not found");

        DizLikedComment dizLikedComment = new()
        {
            UserId = currentUserId,
            CommentId = CommentId,
            DizLikedDate = DateTime.UtcNow
        };

        context.DizLikedComments.Add(dizLikedComment);

        await context.SaveChangesAsync();

        return Created($"api/dizliking/user/{currentUserId}/comment/{CommentId}", comment.cResponse);
    }

    [HttpGet("user/{UserId}/comment/{CommentId}")]
    public async Task<ActionResult> GetDizLikedComment(Guid UserId, Guid CommentId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("current/comments")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserDizLikedComments()
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReDizLikeComment(Guid CommentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DizLikedComment? dizLikedComment = await context.DizLikedComments
            .FirstOrDefaultAsync(dizLikedComment =>
                dizLikedComment.CommentId == CommentId &&
                dizLikedComment.UserId == currentUserId);

        if (dizLikedComment is null)
            return NotFound("DizLike dont placed");

        context.DizLikedComments.Remove(dizLikedComment);

        await context.SaveChangesAsync();

        return NoContent();
    }
}