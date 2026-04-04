using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Extensions;

public static class ContentExtension
{
    public static ContentResponseDto GetContentResponseDto(this Content content)
    {
        return new ContentResponseDto(
            content.Id, content.DomainId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType.ToString(),
            content.VideoMeta?.DurationSeconds, content.ContentUrl, content.PrewievPhotoUrl,
            content.Savers.Count, content.Likers.Count, content.Comments.Count,
            content.DizLikers.Count, content.ViewsCount);
    }
}