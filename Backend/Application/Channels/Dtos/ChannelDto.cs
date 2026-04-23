namespace Application.Channels.Dtos;

public record class ChannelDto(
    Guid Id, string Name, string Slug,
    string? Description, DateTime CreatedDate, string? AvatarPhotoUrl,
    long SubscribersCount, long ContentsCount, long OwnersCount, long TotalLikes, long TotalViews);