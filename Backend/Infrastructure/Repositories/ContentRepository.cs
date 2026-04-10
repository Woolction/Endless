using Domain.Interfaces.Repositories;
using Domain.Rows.Contents;
using Infrastructure.Connector;
using Infrastructure.Context;

namespace Infrastructure.Repositories;

public class ContentRepository : IContentRepository
{
    private readonly DbConnectorFactory connector;
    
    public ContentRepository(DbConnectorFactory connector)
    {
        this.connector = connector;
    }

    public Task<ContentSearchRow[]> SearchContentsByName(string name, bool hasLastSearch, double lastScore)
    {
        return null!;
    }
}