
using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

public class UserVectorsRepository : IUserVectorsRepository
{
    private readonly EndlessContext context;

    public UserVectorsRepository(EndlessContext context)
    {
        this.context = context;
    }

    public async Task AddExistsGenres(UserGenreVector[] userVectors)
    {
        context.UserVectors.AddRange(userVectors);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await context.SaveChangesAsync();
    }
}