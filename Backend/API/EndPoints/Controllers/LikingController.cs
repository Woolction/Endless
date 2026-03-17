using Microsoft.AspNetCore.Authorization;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Microsoft.EntityFrameworkCore;
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
        Content? content = await context.Contents
            .Include(content => content.Creator)
            .FirstOrDefaultAsync(content => content.Id == ContentId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (content is null)
            return BadRequest("Content not found");

        currentUser.LikedContentsCount++;

        content.LikesCount++;
        content.Creator!.ContentsLikesCount++;

        LikedContent likedContent = new()
        {
            UserId = currentUserId,
            ContentId = ContentId,
            LikedDate = DateTime.UtcNow
        };

        context.LikedContents.Add(likedContent);

        await context.SaveChangesAsync();

        return Ok(content.GetContentResponseDto());
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
            .Include(likedContent => likedContent.User)
            .FirstOrDefaultAsync(likedContent =>
                likedContent.ContentId == ContentId &&
                likedContent.UserId == currentUserId);

        if (content is null)
            return BadRequest("Content not found");
        if (likedContent is null)
            return BadRequest("Like dont placed");

        likedContent.User!.LikedContentsCount--;

        content.LikesCount--;
        content.Creator!.ContentsLikesCount--;

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
        Comment? comment = await context.Comments
            .Include(comment => comment.Commentator)
            .FirstOrDefaultAsync(comment => comment.Id == CommentId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (comment is null)
            return BadRequest("Comment not found");

        currentUser.LikedCommentsCount++;

        comment.LikeCount++;
        comment.Commentator!.CommentsLikesCount++;

        LikedComment likedComment = new()
        {
            UserId = currentUserId,
            CommentId = CommentId,
            LikedDate = DateTime.UtcNow
        };

        context.LikedComments.Add(likedComment);

        await context.SaveChangesAsync();

        return Ok(comment.GetCommentResponseDto());
    }

    [HttpDelete("comment/{CommentId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReLikeComment(Guid CommentId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        Comment? comment = await context.Comments
            .Include(comment => comment.Commentator)
            .FirstOrDefaultAsync(comment => comment.Id == CommentId);

        LikedComment? likedComment = await context.LikedComments
            .Include(likedComment => likedComment.User)
            .FirstOrDefaultAsync(likedComment =>
                likedComment.UserId == currentUserId &&
                likedComment.CommentId == CommentId);

        if (comment is null)
            return BadRequest("Comment not found");
        if (likedComment is null)
            return BadRequest("Like dont placed");

        likedComment.User!.LikedCommentsCount--;

        comment.LikeCount--;
        comment.Commentator!.CommentsLikesCount--;

        context.LikedComments.Remove(likedComment);

        await context.SaveChangesAsync();

        return NoContent();
    }

    //DizLikes in other class
}