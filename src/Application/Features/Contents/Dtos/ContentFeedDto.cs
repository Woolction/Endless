using Application.Features.Dtos;

namespace Application.Features.Contents.Dtos;

public record class ContentFeedDto(
    Guid ContentId, Guid? ChannelId, Guid CreatorId,
    string? OwnerName, string? OwnerSlug, PhotoDto? OwnerIcon,
    string Title, string Slug, string? Description, DateTime CreatedDate, string ContentType,
    int DurationSeconds, string? ContentUrl, PhotoDto? Photo, long ViewsCount);