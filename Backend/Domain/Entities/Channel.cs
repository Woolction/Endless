namespace Domain.Entities;

public class Channel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }

    public string? AvatarPhotoUrl { get; set; }

    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }

    public bool IsWound { get; set; }

    public List<ChannelSubscription> Subscribers { get; set; } = new List<ChannelSubscription>();

    public List<ChannelOwner> Owners { get; set; } = new List<ChannelOwner>();
    public List<Content> Contents { get; set; } = new List<Content>();
}