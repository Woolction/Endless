namespace Application.Dtos.Channels;

public record class ChannelDto(
    Guid Id, string Name, string Slug,
    string Description, DateTime CreatedDate, string? AvatarPhotoUrl,
    long SubsrcibersCount, long ContentsCount, long OwnersCount, long TotalLikes, long TotalViews);