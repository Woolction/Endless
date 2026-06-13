using Application.Features.Images;

namespace Application.Features.Contents.Dtos;

public record class ContentFeedDto(
    Guid ContentId, Guid? ChannelId, Guid CreatorId,
    string? OwnerName, string? OwnerSlug, ImageDto? OwnerIcon,
    string Title, string Slug, string? Description, DateTime CreatedDate, string ContentType,
    int DurationSeconds, string? ContentUrl, ImageDto? Photo, long ViewsCount);