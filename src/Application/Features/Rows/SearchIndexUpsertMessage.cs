using MediatR;

namespace Application.Features.Rows;

public record class SearchIndexUpsertMessage(
    string Type, string SearchIndexJson) : IRequest<Result<Null>>;