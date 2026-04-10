using Application.Searchs;
using MediatR;

namespace Application.Contents.Search;

public record class ContentSearchQuery(
    string Name, SearchDto? LastSearch) : IRequest<Result<ContentSearchDto>>;