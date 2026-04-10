namespace Domain.Rows.Users;

public class UserSearchRow
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RegistryData { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AvatarPhotoUrl { get; set; }
    public long TotalLikes { get; set; }
    public long CommentsCount { get; set; }
    public long ContentsCount { get; set; }
    public long FollowersCount { get; set; }
    public long FollowingCount { get; set; }
    public long OwnedChannelsCount { get; set; }
    public long ChannelSubscriptionsCount { get; set; }
    public double Score { get; set; }
}