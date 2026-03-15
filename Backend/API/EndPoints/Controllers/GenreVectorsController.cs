using Microsoft.AspNetCore.Authorization;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Dtos;
using Backend.API.Data.Context;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Models;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenreVectorsController : ControllerBase
{
    private readonly EndlessContext context;

    public GenreVectorsController(EndlessContext context)
    {
        this.context = context;
    }
    
    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateGenreVector(GenreVectorCreateDto createDto)
    {
        GenreInfo genreInfo = await context.GenreInfos.FirstAsync();

        List<Genre> newGenres = new();

        for (int i = 0; i < createDto.GenreNames.Length; i++)
        {
            newGenres.Add(new Genre()
            {
                Name = createDto.GenreNames[i],
                Order = genreInfo.Count
            });

            genreInfo.Count++;
        }

        context.Genres.AddRange(newGenres);

        List<User> users = await context.Users.ToListAsync();

        for (int i = 0; i < users.Count; i++)
        {
            for (int j = 0; j < newGenres.Count; j++)
            {
                context.UserVectors.Add(new UserGenreVector()
                {
                    UserId = users[i].Id,
                    Genre = newGenres[j]
                });
            }
        }

        List<Content> contents = await context.Contents.ToListAsync();

        for (int i = 0; i < contents.Count; i++)
        {
            for (int j = 0; j < newGenres.Count; j++)
            {
                context.ContentVectors.Add(new ContentGenreVector()
                {
                    ContentId = contents[i].Id,
                    Genre = newGenres[j]
                });
            }
        }

        await context.SaveChangesAsync();

        return Ok(newGenres);
    }
}