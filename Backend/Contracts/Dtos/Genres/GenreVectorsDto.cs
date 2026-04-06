namespace Contracts.Dtos.Genres;

public record class GenreVectorsDto(
    UserGenreVectorDto[] UserGenreVectors,
    ContentGenreVectorDto[] ContentGenreVectors);