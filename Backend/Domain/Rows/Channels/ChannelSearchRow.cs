namespace Domain.Rows.Channels;

public class ChannelSearchRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty; 
    public DateTime CreatedDate { get; set; } 
    public string? AvatarPhotoUrl { get; set; }

    public long TotalLikes { get; set; } 
    public long TotalViews { get; set; }

    public long SubscribersCount { get; set; }
    public long OwnersCount { get; set; }
    public long ContentsCount { get; set; }

    public double Score { get; set; }
}