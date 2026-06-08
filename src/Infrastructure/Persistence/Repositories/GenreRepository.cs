using Application.Interfaces.Repositories;
using Application.Interfaces.Db;

namespace Persistence.Repositories;

public class GenreRepository : IGenreRepository
{
    private IDbConnector connector;
    public GenreRepository(IDbConnector connector)
    {
        this.connector = connector;
    }
}