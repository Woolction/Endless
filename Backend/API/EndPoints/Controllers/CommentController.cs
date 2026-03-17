using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly EndlessContext context;

    public CommentController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> SendComment(Guid ContentId, string text)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Id == currentUserId);
        Content? content = await context.Contents
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (content is null)
            return BadRequest("Content not found");

        Comment newComment = new()
        {
            CommentatorId = currentUserId,
            ContentId = ContentId,
            Text = text,
            PublicatedDate = DateTime.UtcNow
        };

        context.Comments.Add(newComment);

        currentUser.CommentsCount++;
        content.CommentsCount++;

        await context.SaveChangesAsync();

        return Ok(new
        {
            CommentResponseDto = newComment.GetCommentResponseDto(),
            User = currentUser.GetUserResponseDto()
        });
    }

    [HttpPut("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> UpdateComment(Guid CommentId, string text)
    {
        Comment? comment = await context.Comments.FindAsync(CommentId);

        if (comment is null)
            return BadRequest("Comment not found");

        comment.Text = text;

        await context.SaveChangesAsync();

        return Ok(comment.GetCommentResponseDto());
    }

    [HttpDelete("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> DeleteComment(Guid CommentId)
    {
        Comment? comment = await context.Comments
            .Include(comment => comment.Commentator)
            .Include(comment => comment.Content)
            .FirstOrDefaultAsync(comment => comment.Id == CommentId);

        if (comment is null)
            return BadRequest("Comment not found");

        comment.Commentator!.CommentsCount--;
        comment.Content!.CommentsCount--;

        context.Comments.Remove(comment);

        await context.SaveChangesAsync();

        return NoContent();
    }
}