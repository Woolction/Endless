namespace Backend.API.Dtos;

public record class DomainSearchResponseDto(
    DomainResponseDto[] DomainsDto, double? Similariry);