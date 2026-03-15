namespace Backend.API.Dtos;

public record class SearchRequestDto(string Name, SearchDto? LastSearch);