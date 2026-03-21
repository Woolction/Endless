using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Extensions;

public static class UserExtension
{
    public static UserResponseDto GetUserResponseDto(this User user)
    {
        return new(
            user.Id,
            user.Name,
            "@" + user.Slug,
            user.Description ?? "",
            user.RegistryData,
            user.Email, user.Role.ToString(),
            user.AvatarPhotoUrl,
            user.TotalLikes,
            user.ContentsLikesCount,
            user.CommentsLikesCount,
            user.CommentsCount,
            user.ContentsCount,
            user.FollowersCount,
            user.FollowingCount,
            user.OwnedDomainsCount,
            user.SubscripedDomainsCount);
    }
}