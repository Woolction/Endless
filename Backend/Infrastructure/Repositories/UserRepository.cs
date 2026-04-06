using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;
using Domain.Entities;
using Infrastructure.Connector;
using System.Data;
using Dapper;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly EndlessContext context;
    private readonly DbConnectorFactory dbConnector;

    public UserRepository(EndlessContext context, DbConnectorFactory dbConnector)
    {
        this.context = context;

        this.dbConnector = dbConnector;
    }
    
    public void AddUser(User user)
    {
        context.Users.Add(user);
    }

    public async Task<bool> AnyUserByEmail(string email)
    {
        return await context.Users.AnyAsync(user => user.Email == email);
    }

    public async Task<User?> GetUserByEmail(string email)
    {
        return await context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User?> GetUserByRefreshToken(string refreshToken)
    {
        return await context.Users
            .Include(u => u.RefreshToken)
            .FirstOrDefaultAsync(user =>
                user.RefreshToken != null && user.RefreshToken.Token == refreshToken);
    }

    public async Task<User[]> GetUserSearchByName()
    {
        using IDbConnection db = dbConnector.CreateConnection();

        string sql = @"SELECT * FROM Users";

        IEnumerable<User> users = await db.QueryAsync<User>(sql);
        
        return [.. users];
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}