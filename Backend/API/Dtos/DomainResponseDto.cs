namespace Backend.API.Dtos;

public record class DomainResponseDto(
    Guid Id, string Name, string Slug,
    string Description, DateTime CreatedDate, string? AvatarPhotoUrl,
    long SubsrcibersCount, long OwnersCount, long TotalLikes, long TotalViews);