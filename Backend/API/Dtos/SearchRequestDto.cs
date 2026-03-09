namespace Backend.API.Dtos;

public record class SearchRequestDto(
    string? Slug, DateTime? LastCreatedDate, int Take);