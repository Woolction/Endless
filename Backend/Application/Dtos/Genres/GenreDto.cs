namespace Application.Dtos.Genres;

public record class GenreDto(
    Guid Id, string Name, int Order);