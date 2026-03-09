namespace Backend.API.Dtos;

public record class DomainSearchRequestDto(
    string Slug, int Take, double? LastSimilarity);