namespace Application.Genres.CreateMany;

public record class GenreVectorCreateCommand(
    params string[] GenreNames);