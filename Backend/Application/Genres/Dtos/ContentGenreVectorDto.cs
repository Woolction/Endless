namespace Application.Genres.Dtos;

public record class ContentGenreVectorDto(
    GenreDto Genre, float AuthorVector, float AudienceVector,
    float FinalVector);