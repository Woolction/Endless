namespace Backend.API.Dtos;

public record class DomainSearchRequestDto(
    string Name, int Take, double? LastSimilarity);