namespace Backend.API.Dtos;

public record class CommentResponseDto(
    Guid CommentId,
    string? Text, DateTime PublicatedDate,
    long LikeCount, long DizLikeCount, long ViewsCount);