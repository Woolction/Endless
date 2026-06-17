using Domain.Common.Enums;

namespace Domain.Entities;

public class Content
{
    public Guid Id { get; set; }

    public Guid? ChannelId { get; set; }
    public Channel? Channel { get; set; }

    public Guid CreatorId { get; set; }
    public User? Creator { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public ContentType ContentType { get; set; } = ContentType.Video;
    public ContentStatusType StatusType { get; set; } = ContentStatusType.None;

    public VideoMeta Meta { get; set; } = new();

    public long ViewsCount { get; set; }

    public double RandomKey { get; set; }

    public bool IsWound { get; set; }

    public List<ContentGenreVector> Vectors { get; set; } = new List<ContentGenreVector>();

    public List<UserInteractionContent> UsersInteration { get; set; } = new List<UserInteractionContent>();

    public List<SavedContent> Savers { get; set; } = new List<SavedContent>();
    public List<LikedContent> Likers { get; set; } = new List<LikedContent>();
    public List<DisLikedContent> DisLikers { get; set; } = new List<DisLikedContent>();

    public List<Comment> Comments { get; set; } = new List<Comment>();
}