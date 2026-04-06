namespace Application.Commands.Genres;

public record class GenreVectorCreateCommand(
    params string[] GenreNames);