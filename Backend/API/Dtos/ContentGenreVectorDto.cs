namespace Backend.API.Dtos;

public record class ContentGenreVectorDto(
    GenreResponseDto Genre,
    float AuthorVector,
    float AudienceVector,
    float FinalVector
    );