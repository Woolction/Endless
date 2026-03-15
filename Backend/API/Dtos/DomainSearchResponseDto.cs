namespace Backend.API.Dtos;

public record class DomainSearchResponseDto(
    DomainResponseDto[] DomainsDto, SearchDto? Similariry);