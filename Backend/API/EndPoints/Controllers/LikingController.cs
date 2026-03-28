using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LikingController : ControllerBase
{
    private readonly EndlessContext context;

    public LikingController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentResponseDto>> LikeContent(Guid ContentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var content = await context.Contents
            .Select(content => new { 
                c = content, cResponse = new ContentResponseDto(
                    content.Id, content.DomainId, content.CreatorId,
                    content.Title, content.Slug, content.Description,
                    content.CreatedDate, content.ContentType.ToString(),
                    content.VideoMeta != null ? content.VideoMeta.DurationSeconds : 0,
                    content.ContentUrl, content.PrewievPhotoUrl, content.Savers.Count,
                    content.Likers.Count, content.Comments.Count, content.DizLikers.Count,
                    content.ViewsCount)})
            .FirstOrDefaultAsync(content => content.c.Id == ContentId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (content is null || content.c is null)
            return BadRequest("Content not found");

        LikedContent likedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            LikedDate = DateTime.UtcNow
        };

        context.LikedContents.Add(likedContent);

        await context.SaveChangesAsync();

        return Ok(content.cResponse);
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
            return BadRequest("Content not found");
        if (likedContent is null)
            return BadRequest("Like dont placed");

        context.LikedContents.Remove(likedContent);

        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentResponseDto>> LikeComment(Guid CommentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var comment = await context.Comments
            .Select(comment => new {
                c = comment, cResponse = new CommentResponseDto(
                    comment.Id,
                    comment.Text,
                    comment.PublicatedDate,
                    comment.Likers.Count,
                    comment.DizLikers.Count,
                    comment.ViewsCount)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(comment => comment.c.Id == CommentId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (comment is null || comment.c is null)
            return BadRequest("Comment not found");

        LikedComment likedComment = new()
        {
            UserId = currentUserId,
            CommentId = CommentId,
            LikedDate = DateTime.UtcNow
        };

        context.LikedComments.Add(likedComment);

        await context.SaveChangesAsync();

        return Ok(comment.cResponse);
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
            return BadRequest("Comment not found");
        if (likedComment is null)
            return BadRequest("Like dont placed");

        context.LikedComments.Remove(likedComment);

        await context.SaveChangesAsync();

        return NoContent();
    }
}