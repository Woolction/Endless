using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Extensions;

public static class CommentExtension
{
    public static CommentResponseDto GetCommentResponseDto(this Comment comment)
    {
        return new(
            comment.Id,
            comment.Text,
            comment.PublicatedDate,
            0, 0, comment.ViewsCount);
    }
}