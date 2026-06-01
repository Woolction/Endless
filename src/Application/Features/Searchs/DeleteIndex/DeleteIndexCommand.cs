using MediatR;

namespace Application.Features.Searchs.DeleteIndex;

public record class DeleteIndexCommand(
    string IndexName) : IRequest<Result<Null>>;