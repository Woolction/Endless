using Backend.API.Data.Models;
using Backend.API.Dtos;

namespace Backend.API.Extensions;

public static class DomainExtension
{
    public static DomainResponseDto GetDomainResponseDto(this Domain domain)
    {
        return new(
            domain.Id,
            domain.Name,
            "@" + domain.Slug,
            domain.Description ?? "",
            domain.CreatedDate,
            domain.AvatarPhotoUrl,
            domain.SubscribersCount,
            domain.OwnersCount,
            domain.TotalLikes,
            domain.TotalViews);
    }
}