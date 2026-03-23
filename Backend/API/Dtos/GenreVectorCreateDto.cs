namespace Backend.API.Dtos;

public record class GenreVectorCreateDto(
    params string[] GenreNames);