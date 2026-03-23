namespace Backend.API.Dtos;

public record class UserGenreVectorDto(
    GenreResponseDto Genre, float Value);