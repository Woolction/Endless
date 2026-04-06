using Contracts.Dtos.Contents;
using Domain.Entities;

namespace API.Extensions;

public static class ContentExtension
{
    public static ContentDto GetContentDto(this Content content)
    {
        return new ContentDto(
            content.Id, content.ChannelId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType.ToString(),
            content.VideoMeta?.DurationSeconds, content.ContentUrl, content.PrewievPhotoUrl,
            0, 0, 0, 0, 0);
    }
}