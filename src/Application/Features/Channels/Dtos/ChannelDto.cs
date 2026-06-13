using Application.Features.Images;

namespace Application.Features.Channels.Dtos;

public record class ChannelDto(
    Guid Id, string Name, string Slug,
    string? Description, DateTime CreatedDate, ImageDto Icon,
    long SubscribersCount, long ContentsCount, long OwnersCount, long TotalLikes, long TotalViews);