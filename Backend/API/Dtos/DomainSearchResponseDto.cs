namespace Backend.API.Dtos;

public record class DomainSearchResponseDto(
    Guid Id, string Name, string Slug,
    string Description, DateTime CreatedDate,
    long SubsrcibersCount, long OwnersCount, long TotalLikes,
    long TotalViews, double Similariry);