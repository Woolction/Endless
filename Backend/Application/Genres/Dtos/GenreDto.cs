namespace Application.Genres.Dtos;

public record class GenreDto(
    Guid Id, string Name, int Order);