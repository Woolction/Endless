using System.Data;

namespace Application.Interfaces.Db;

public interface IDbConnector
{
    IDbConnection CreateConnection();
}