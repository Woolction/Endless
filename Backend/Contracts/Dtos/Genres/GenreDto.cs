namespace Contracts.Dtos.Genres;

public record class GenreDto(
    Guid Id, string Name, int Order);