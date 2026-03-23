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

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))] //<SendCommentDto>
    public async Task<ActionResult> SendComment(Guid ContentId, [FromBody] string text)
    {
        Guid currentUserId = this.GetIDFromClaim();

        var currentUser = await context.Users
            .Select(user => new {
                u = user, uResponse = new UserResponseDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), user.AvatarPhotoUrl, user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedDomains.Count, user.SubscripedDomains.Count)})
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.u.Id == currentUserId);
        Content? content = await context.Contents
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (currentUser is null || currentUser.u is null)
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

        await context.SaveChangesAsync();

        return Ok(new SendCommentDto(
            newComment.GetCommentResponseDto(), currentUser.uResponse));
    }

    [HttpPut("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentResponseDto>> UpdateComment(Guid CommentId, string text)
    {
        var comment = await context.Comments
            .Select(comment => new {
                c = comment, cResponse = new CommentResponseDto(
                    comment.Text,
                    comment.PublicatedDate,
                    comment.Likers.Count,
                    comment.DizLikers.Count,
                    comment.ViewsCount)})
            .FirstOrDefaultAsync(comment => comment.c.Id == CommentId);

        if (comment is null || comment.c is null)
            return BadRequest("Comment not found");

        comment.c.Text = text;

        await context.SaveChangesAsync();

        return Ok(comment.cResponse);
    }

    [HttpDelete("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> DeleteComment(Guid CommentId)
    {
        Comment? comment = await context.Comments
            .FirstOrDefaultAsync(comment => comment.Id == CommentId);

        if (comment is null)
            return BadRequest("Comment not found");

        context.Comments.Remove(comment);

        await context.SaveChangesAsync();

        return NoContent();
    }
}