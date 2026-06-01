namespace Application.Features.Genres.Dtos;

public record class UserGenreVectorDto(
    GenreDto Genre, float Value);