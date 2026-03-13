using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Extensions;

public static class ContentExtension
{
    public static ContentRecoDto GetContentRecoDto(this Content content)
    {
        return new ContentRecoDto(
            content.Id, content.DomainId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType, Random.Shared.NextDouble(),
            content.VideoMeta, content.ContentUrl, content.PrewievPhotoUrl,
            content.SavesCount, content.LikesCount, content.CommentsCount,
            content.DizLikesCount, content.ViewsCount);
    }

    public static ContentResponseDto GetContentResponseDto(this Content content)
    {
        return new ContentResponseDto(
            content.Id, content.DomainId, content.CreatorId,
            content.Title, content.Slug, content.Description,
            content.CreatedDate, content.ContentType,
            content.VideoMeta, content.ContentUrl, content.PrewievPhotoUrl,
            content.SavesCount, content.LikesCount, content.CommentsCount,
            content.DizLikesCount, content.ViewsCount);
    }
}