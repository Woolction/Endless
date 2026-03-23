namespace Backend.API.Dtos;

public record class CommentResponseDto(
    string? Text, DateTime PublicatedDate,
    long LikeCount, long DizLikeCount, long ViewsCount);