namespace Application.Dtos.Genres;

public record class GenreVectorsDto(
    UserGenreVectorDto[] UserGenreVectors,
    ContentGenreVectorDto[] ContentGenreVectors);