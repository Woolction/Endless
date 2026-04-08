namespace Application.Dtos.Genres;

public record class ContentGenreVectorDto(
    GenreDto Genre, float AuthorVector, float AudienceVector,
    float FinalVector);