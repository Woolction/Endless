namespace Backend.API.Dtos;

public record class ContentRecoDto(
    Guid ContentId, Guid DomainId, Guid CreatorId, string Title,
    Guid Slug, string? Description, DateTime CreatedDate, string ContentType,
    double RandomKey, int? DurationSeconds, string? ContentUrl, string? PrewievPhotoUrl,
    long SavesCount, long LikesCount, long CommentsCount, long DizLikesCount, long ViewsCount);