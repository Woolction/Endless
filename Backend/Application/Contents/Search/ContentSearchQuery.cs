using Application.Searchs;
using MediatR;

namespace Application.Contents.Search;

public record class ContentSearchQuery(
    string Name, double? LastScore) : IRequest<Result<SearchedContentDto[]>>;