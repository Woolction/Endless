namespace Backend.API.Dtos;

public record class UserSearchRequestDto(
    string Name, SearchDto? LastSearch);