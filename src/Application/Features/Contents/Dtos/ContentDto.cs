using Application.Features.Dtos;

namespace Application.Features.Contents.Dtos;

public record class ContentDto(
    Guid ContentId, Guid? ChannelId, Guid CreatorId, string Title,
    string Slug, string? Description, DateTime CreatedDate, string ContentType,
    int DurationSeconds, string? ContentUrl, PhotoDto? Photo, long SavesCount,
    long LikesCount, long CommentsCount, long DisLikersCount, long ViewsCount);