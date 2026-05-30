using MediatR;

namespace Application.Features.Genres.CreateMany;

public record class GenreVectorCreateCommand(
    params string[] GenreNames) : IRequest;