namespace Application.Features.Genres.Dtos;

public record class GenreVectorsDto(
    UserGenreVectorDto[] UserGenreVectors,
    ContentGenreVectorDto[] ContentGenreVectors);