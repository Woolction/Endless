using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Domain.Entities;
using Infrastructure.Connector;

public class UserVectorsRepository : IUserVectorsRepository
{
    private readonly DbConnectorFactory connector;

    public UserVectorsRepository(DbConnectorFactory connector)
    {
        this.connector = connector;
    }
}