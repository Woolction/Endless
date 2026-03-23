using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Dtos;

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
        Content? content = await context.Contents.FirstOrDefaultAsync(content => content.Id == ContentId);

        if (!hasUser)
            return BadRequest("User not found");
        if (content is null)
            return BadRequest("Content not found");

        DizLikedContent dizLikedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            DizLikedDate = DateTime.UtcNow
        };

        context.DizLikedContents.Add(dizLikedContent);

        await context.SaveChangesAsync();

        return Ok(content.GetContentResponseDto());
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
            return BadRequest("Like dont placed");

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
        Comment? comment = await context.Comments.FirstOrDefaultAsync(comment => comment.Id == CommentId);

        if (!hasUser)
            return BadRequest("User not found");
        if (comment is null)
            return BadRequest("Comment not found");

        DizLikedComment dizLikedComment = new()
        {
            UserId = currentUserId,
            CommentId = CommentId,
            DizLikedDate = DateTime.UtcNow
        };

        context.DizLikedComments.Add(dizLikedComment);

        await context.SaveChangesAsync();

        return Ok(comment.GetCommentResponseDto());
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
            return BadRequest("DizLike dont placed");

        context.DizLikedComments.Remove(dizLikedComment);

        await context.SaveChangesAsync();

        return NoContent();
    }
}