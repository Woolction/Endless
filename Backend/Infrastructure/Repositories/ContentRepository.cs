using Domain.Interfaces.Repositories;
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

    public Task<dynamic[]> SearchContentsByName(string name, bool hasLastSearch, double lastScore)
    {
        return null!;
    }
}