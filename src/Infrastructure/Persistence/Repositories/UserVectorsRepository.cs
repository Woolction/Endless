using Application.Interfaces.Db;
using Application.Interfaces.Repositories;

namespace Persistence.Repositories;

public class UserVectorsRepository : IUserVectorsRepository
{
    private readonly IDbConnector connector;

    public UserVectorsRepository(IDbConnector connector)
    {
        this.connector = connector;
    }
}