using Microsoft.Extensions.Configuration;
using System.Data;
using Npgsql;

namespace Infrastructure.Connector;

public class DbConnectorFactory
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