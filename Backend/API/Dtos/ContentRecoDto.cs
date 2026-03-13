using Backend.API.Data.Components;
using Backend.API.Data.Models;

namespace Backend.API.Dtos;

public record class ContentRecoDto(
    Guid ContentId, Guid DomainId, Guid CreatorId,
    string Title, Guid Slug, string? Description,
    DateTime CreatedDate, ContentType ContentType, double RandomKey,
    VideoMetaData? VideoMeta, string? ContentUrl, string? PrewievPhotoUrl,
    long SavesCount, long LikesCount, long CommentsCount, long DizLikesCount, long ViewsCount);