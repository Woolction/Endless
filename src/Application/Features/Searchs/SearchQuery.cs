namespace Application.Features.Searchs;

public record class SearchQuery(
    string Name, SearchDto? LastSearch);