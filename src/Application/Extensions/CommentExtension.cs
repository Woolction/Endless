using Application.Features.Comments.Dtos;
using Domain.Entities;

namespace Application.Extensions;

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