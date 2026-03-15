using Amazon.S3.Model;
using Backend.API.Data.Models;
using Backend.API.Dtos;
using Microsoft.EntityFrameworkCore;

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
            user.Email, user.Role,
            user.AvatarPhotoUrl,
            user.TotalLikes,
            user.CommentsCount,
            user.ContentsCount,
            user.FollowersCount,
            user.FollowingCount,
            user.OwnedDomainsCount,
            user.DomainSubscriptionsCount);
    }
}