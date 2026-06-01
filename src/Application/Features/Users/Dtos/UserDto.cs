using Application.Features.Dtos;

namespace Application.Features.Users.Dtos;

public record class UserDto(
    Guid Id, string Name, string Slug, string? Description,
    DateTime RegistryDate, string Email, string Role, PhotoDto Icon,
    long TotalLikes, long CommentsCount, long ContentsCount, long FollowersCount,
    long FollowingCount, long OwnedChannelsCount, long ChannelSubscriptionsCount);