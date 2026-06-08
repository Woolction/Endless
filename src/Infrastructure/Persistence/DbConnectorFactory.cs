using Microsoft.Extensions.Configuration;
using System.Data;
using Npgsql;
using Application.Interfaces.Db;

namespace Persistence;

public class DbConnectorFactory : IDbConnector
{
    private readonly string DbKey = "";

    public DbConnectorFactory(IConfiguration configuration)
    {
        DbKey = configuration.GetConnectionString("DB")!;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(DbKey);
    }
}