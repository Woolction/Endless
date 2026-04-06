using Contracts.Dtos.Searchs;

namespace Application.Queries.Searchs;

public record class SearchQuery(
    string Name, SearchDto? LastSearch);