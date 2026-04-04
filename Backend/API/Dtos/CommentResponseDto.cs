namespace Backend.API.Dtos;

public record class CommentResponseDto(
    Guid CommentId,
    string? Text, DateTime PublicatedDate,
    long LikeCount, long DisLikersCount, long ViewsCount);