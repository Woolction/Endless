
using System.ComponentModel.DataAnnotations.Schema;
using Backend.API.Data.Components;

namespace Backend.API.Data.Models;

public class Content
{
    public Guid Id { get; set; }

    public Guid DomainId { get; set; }
    [ForeignKey(nameof(DomainId))] public Domain? Domain { get; set; }

    public Guid CreatorId { get; set; }
    [ForeignKey(nameof(CreatorId))] public User? Creator { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Video;

    public VideoMetaData? VideoMeta { get; set; }

    public string? ContentUrl { get; set; }
    public string? PrewievPhotoUrl { get; set; }

    public long SavesCount { get; set; }
    public long LikesCount { get; set; }
    public long CommentsCount { get; set; }
    public long DizLikesCount { get; set; }
    public long ViewsCount { get; set; }

    public long VectorsCount { get; set; }

    public double RandomKey { get; set; }

    public List<ContentGenreVector> Vectors { get; set; } = new List<ContentGenreVector>();

    public List<UserInterationContent> UsersInteration { get; set; } = new List<UserInterationContent>();

    public List<SavedContent> Savers { get; set; } = new List<SavedContent>();
    public List<LikedContent> Likers { get; set; } = new List<LikedContent>();

    public List<Comment> Comments { get; set; } = new List<Comment>();
}