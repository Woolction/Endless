namespace Domain.Interfaces.Repositories;

public interface IContentRepository
{
    Task<dynamic[]> SearchContentsByName(string name, bool hasLastSearch, double lastScore);
}