namespace Backend.API.Dtos;

public record class DomainResponseDto(
    Guid Id, string Name, string Slug,
    string Description, DateTime CreatedDate,
    long SubsrcibersCount, long TotalLikes, long TotalViews);