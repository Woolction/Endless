using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Handlers;

public class ContentCreateHandler
{
    private readonly IAppDbContext context;

    public ContentCreateHandler(IAppDbContext context)
    {
        this.context = context;
    }
    
    public async Task Handle()
    {
        await context.Genres
            .Select(genre => new ContentGenreVector()
            {
                Content = new Content(),
                GenreId = genre.Id
            })
            .AsNoTracking()
            .ToArrayAsync();
    }
}