using Application.Comments.Dtos;
using Domain.Entities;

namespace API.Extensions;

public static class CommentExtension
{
    public static CommentDto GetCommentDto(this Comment comment)
    {
        return new(
            comment.Id,
            comment.Text,
            comment.PublicatedDate,
            0, 0, comment.ViewsCount);
    }
}