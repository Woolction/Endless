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
            user.Comments.Count,
            user.Contents.Count,
            user.Followers.Count,
            user.Following.Count,
            user.OwnedDomains.Count,
            user.SubscripedDomains.Count);
    }
}