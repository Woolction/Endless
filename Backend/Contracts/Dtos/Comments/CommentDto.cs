namespace Contracts.Dtos.Comments;

public record class CommentDto(
    Guid CommentId, string? Text, DateTime PublicatedDate,
    long LikeCount, long DisLikersCount, long ViewsCount);