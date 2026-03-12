using Backend.API.Data.Components;
using Backend.API.Data.Models;

namespace Backend.API.Dtos;

public record class ContentResponseDto(
    Guid ContentId, Guid DomainId, Guid CreatorId,
    string Title, string Slug, string? Description,
    DateTime CreatedDate, ContentType ContentType,
    VideoMetaData? VideoMeta, string? ContentUrl, string? PrewievPhotoUrl,
    long SavesCount, long LikesCount, long CommentsCount, long DizLikesCount, long ViewsCount);