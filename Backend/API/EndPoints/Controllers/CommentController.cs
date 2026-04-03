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
public class CommentController : ControllerBase
{
    private readonly EndlessContext context;

    public CommentController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpGet("content/{ContentId}")]
    public async Task<ActionResult<SendCommentDto[]>> GetCommentWithContnet(Guid ContentId)
    {
        bool hasContent = await context.Contents
            .AsNoTracking().AnyAsync(content => content.Id == ContentId);

        if (!hasContent)
            return NotFound("Content not found");

        var comments = await context.Comments
            .Where(comment => comment.ContentId == ContentId)
            .Include(comment => comment.Commentator)
            .Select(comment => new SendCommentDto(
                new CommentResponseDto(
                    comment.Id, comment.Text,
                    comment.PublicatedDate, comment.Likers.Count,
                    comment.DizLikers.Count, comment.ViewsCount),
                new UserResponseDto(
                    comment.Commentator!.Id, comment.Commentator!.Name, "@" + comment.Commentator!.Slug,
                    comment.Commentator!.Description ?? "", comment.Commentator!.RegistryData, comment.Commentator!.Email,
                    comment.Commentator!.Role.ToString(), comment.Commentator!.AvatarPhotoUrl, comment.Commentator!.TotalLikes,
                    comment.Commentator!.Comments.Count, comment.Commentator!.Contents.Count, comment.Commentator!.Followers.Count,
                    comment.Commentator!.Following.Count, comment.Commentator!.OwnedDomains.Count, comment.Commentator!.SubscripedDomains.Count)))
            .ToArrayAsync();

        return Ok(comments);
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))] 
    public async Task<ActionResult<SendCommentDto>> SendComment(Guid ContentId, [FromBody] CreateCommentDto commentDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        var currentUser = await context.Users
            .Select(user => new
            {
                u = user,
                uResponse = new UserResponseDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.u.Id == currentUserId);
            
        Content? content = await context.Contents
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (currentUser is null || currentUser.u is null)
            return NotFound("User not found");
        if (content is null)
            return NotFound("Content not found");

        Comment newComment = new()
        {
            CommentatorId = currentUserId,
            ContentId = ContentId,
            Text = commentDto.Text,
            PublicatedDate = DateTime.UtcNow
        };

        context.Comments.Add(newComment);

        await context.SaveChangesAsync();

        return Created($"api/comment/{newComment.Id}", new SendCommentDto(
            newComment.GetCommentResponseDto(), currentUser.uResponse));
    }

    [HttpPut("{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentResponseDto>> UpdateComment(Guid CommentId, string text)
    {
        var comment = await context.Comments
            .Where(comment => comment.Id == CommentId)
            .Select(comment => new {
                c = comment,
                cResponse = new CommentResponseDto(
                    comment.Id,
                    comment.Text,
                    comment.PublicatedDate,
                    comment.Likers.Count,
                    comment.DizLikers.Count,
                    comment.ViewsCount)
            })
            .FirstOrDefaultAsync();

        if (comment is null || comment.c is null)
            return NotFound("Comment not found");

        comment.c.Text = text;

        await context.SaveChangesAsync();

        return Ok(comment.cResponse);
    }

    [HttpDelete("{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> DeleteComment(Guid CommentId)
    {
        Comment? comment = await context.Comments
            .FirstOrDefaultAsync(comment => comment.Id == CommentId);

        if (comment is null)
            return NotFound("Comment not found");

        context.Comments.Remove(comment);

        await context.SaveChangesAsync();

        return NoContent();
    }
}