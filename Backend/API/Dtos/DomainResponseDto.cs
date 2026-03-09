namespace Backend.API.Dtos;

public record class DomainResponseDto(Guid Id,string Name, string Slug, DateTime CreatedDate);