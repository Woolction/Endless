using Domain.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Context;
using Domain.Entities;

namespace Infrastructure.Repositories;

public class GenreRepository : IGenreRepository
{
    private readonly EndlessContext context;
    public GenreRepository(EndlessContext context)
    {
        this.context = context;
    }

    public async Task<UserGenreVector[]> CreateGenresForUser(User user)
    {
        return await context.Genres
            .Select(genre => new UserGenreVector()
            {
                User = user,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToArrayAsync();
    }

    public async Task<ContentGenreVector[]> CreateGenresForContent(Content content)
    {
        return await context.Genres
            .Select(genre => new ContentGenreVector()
            {
                Content = content,
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToArrayAsync();
    }
}