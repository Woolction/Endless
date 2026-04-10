using Domain.Rows.Contents;

namespace Domain.Interfaces.Repositories;

public interface IContentRepository
{
    Task<ContentSearchRow[]> SearchContentsByName(string name, bool hasLastSearch, double lastScore);
}