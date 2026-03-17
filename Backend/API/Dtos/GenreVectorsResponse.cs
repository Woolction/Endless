namespace Backend.API.Dtos;

public record class GenreVectorsResponse(
    UserGenreVectorDto[] UserGenreVectors,
    ContentGenreVectorDto[] ContentGenreVectors
    );