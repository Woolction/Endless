using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Db;
using Application.Features.Comments.Create;
using Application.Features.Comments.Dtos;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Users.Dtos;
using Application.Utilities;
using Domain.Common.Enums;
using Domain.Entities;
using API.Extensions;
using Application.Features.Comments.Update;
using Application.Features.Dtos;
using Domain.Rows.Contents;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentController : ControllerBase
{
    private readonly IAppDbContext context;

    private readonly ILogger<CommentController> logger;

    public CommentController(IAppDbContext context, ILogger<CommentController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpGet("content/{ContentId}")]
    public async Task<ActionResult<CommentSendedDto[]>> GetCommentByContentId(Guid ContentId)
    {
        bool hasContent = await context.Contents
            .AsNoTracking().AnyAsync(content => content.Id == ContentId);

        if (!hasContent)
            return NotFound("Content not found");

        var comments = await context.Comments
            .Where(comment => comment.ContentId == ContentId)
            .Select(comment => new CommentSendedDto(
                new CommentDto(
                    comment.Id, comment.Text,
                    comment.PublicatedDate, comment.Likers.Count,
                    comment.DisLikers.Count, comment.ViewsCount),
                new UserDto(
                    comment.Commentator.Id, comment.Commentator.Name, "@" + comment.Commentator.Slug,
                    comment.Commentator.Description ?? "", comment.Commentator.RegistryData, comment.Commentator.Email,
                    comment.Commentator.Role.ToString(), new PhotoDto(
                        new PhotoVariants(
                            comment.Commentator.UserMeta.IconBase,
                            comment.Commentator.UserMeta.Small,
                            comment.Commentator.UserMeta.Medium,
                            comment.Commentator.UserMeta.Large),
                        comment.Commentator.UserMeta.R,
                        comment.Commentator.UserMeta.G,
                        comment.Commentator.UserMeta.B), comment.Commentator.TotalLikes,
                    comment.Commentator.Comments.Count, comment.Commentator.Contents.Count, comment.Commentator.Followers.Count,
                    comment.Commentator.Following.Count, comment.Commentator.OwnedChannels.Count, comment.Commentator.SubscripedChannels.Count)))
            .ToArrayAsync();

        logger.LogInformation("Returned {Count} comment in content {ContentId}",
            comments.Length, ContentId);

        return Ok(comments);
    }

    [HttpGet("{CommentId}")]
    public async Task<ActionResult<CommentSendedDto>> GetCommentById(Guid CommentId)
    {
        return Ok();
    }

    [HttpPost("content/{ContentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentSendedDto>> SendComment(Guid ContentId, [FromBody] CreateCommentCommand command)
    {
        Guid currentUserId = this.GetIDFromClaim();

        var currentUser = await context.Users
            .Select(user => new
            {
                u = user,
                uResponse = new UserDto(
                    user.Id, user.Name, "@" + user.Slug,
                    user.Description ?? "", user.RegistryData, user.Email,
                    user.Role.ToString(), new PhotoDto(
                        new PhotoVariants(
                            user.UserMeta.IconBase,
                            user.UserMeta.Small,
                            user.UserMeta.Medium,
                            user.UserMeta.Large),
                        user.UserMeta.R,
                        user.UserMeta.G,
                        user.UserMeta.B), user.TotalLikes,
                    user.Comments.Count, user.Contents.Count, user.Followers.Count,
                    user.Following.Count, user.OwnedChannels.Count, user.SubscripedChannels.Count)
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
            Text = command.Text,
            PublicatedDate = DateTime.UtcNow
        };

        context.Comments.Add(newComment);

        await context.SaveChangesAsync();

        logger.LogInformation("Created comment {CommentId} in content {ContentId}",
            newComment.Id, ContentId);

        return Created($"api/comment/{newComment.Id}", new CommentSendedDto(
            newComment.GetCommentDto(), currentUser.uResponse));
    }

    [HttpPost("{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentSendedDto>> SendCommentToComment(Guid CommentId)
    {
        return Created("api / comment /{newComment.Id}", null);
    }

    [HttpPut("{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<CommentDto>> UpdateComment(Guid CommentId, UpdateCommentCommand command)
    {
        var comment = await context.Comments
            .Select(comment => new
            {
                c = comment,
                LikersCount = comment.Likers.Count,
                DisLikersCount = comment.DisLikers.Count
            })
            .FirstOrDefaultAsync(comment => comment.c.Id == CommentId);

        if (comment is null || comment.c is null)
            return NotFound("Comment not found");

        string oldText = comment.c.Text.Sanitize();

        comment.c.Text = command.Text;

        CommentDto responseDto = new(
            comment.c.Id, comment.c.Text,
            comment.c.PublicatedDate, comment.LikersCount,
            comment.DisLikersCount, comment.c.ViewsCount);

        await context.SaveChangesAsync();

        logger.LogInformation(
            "Comment {CommentId} updated successfully. Changed the text from: {OldText} to: {NewText}",
            CommentId, oldText, command.Text.Sanitize());

        return Ok(responseDto);
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

        logger.LogInformation("Comment {CommentId} deleted",
            CommentId);

        return NoContent();
    }
}