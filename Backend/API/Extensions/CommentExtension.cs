using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Extensions;

public static class CommentExtension
{
    public static CommentResponseDto GetCommentResponseDto(this Comment comment)
    {
        return new(
            comment.Text,
            comment.PublicatedDate,
            comment.Likers.Count,
            comment.DizLikers.Count,
            comment.ViewsCount);
    }
}