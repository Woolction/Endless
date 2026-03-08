
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

    public long DizLikeCount { get; set; }
    public long ViewsCount { get; set; }

    public List<SavedContent> ContentSaveds { get; set; } = new List<SavedContent>();
    public List<LikedContent> ContentLikeds { get; set; } = new List<LikedContent>();

    public List<Comment> Comments { get; set; } = new List<Comment>();
}