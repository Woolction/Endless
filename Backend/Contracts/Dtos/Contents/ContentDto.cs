namespace Contracts.Dtos.Contents;

public record class ContentDto(
    Guid ContentId, Guid? ChannelId, Guid CreatorId, string Title,
    Guid Slug, string? Description, DateTime CreatedDate, string ContentType,
    int? DurationSeconds, string? ContentUrl, string? PrewievPhotoUrl, long SavesCount,
    long LikesCount, long CommentsCount, long DisLikersCount, long ViewsCount);