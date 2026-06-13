namespace Application.Features.Contents.Update;

public record class ContentUpdateDto(
    Guid ContentId, Guid? ChannelId, Guid CreatorId, string Title,
    string Slug, string? Description, DateTime CreatedDate, string ContentType,
    int DurationSeconds, string? ContentUrl, string Process);