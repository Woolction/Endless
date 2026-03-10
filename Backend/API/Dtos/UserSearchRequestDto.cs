namespace Backend.API.Dtos;

public record class UserSearchRequestDto(
    string Name, double? LastSimilarity);