using MediatR;

namespace Application.Searchs.DeleteIndex;

public record class DeleteIndexCommand(
    string IndexName) : IRequest<Result<Null>>;