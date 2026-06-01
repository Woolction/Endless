using MediatR;

namespace Application.Features.Contents.Search;

public record class ContentSearchQuery(
    string Name, double? LastScore) : IRequest<Result<SearchedContentDto[]>>;