using Application.Commands.Genres;
using Application.Dtos.Genres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Domain.Common;
using Infrastructure.Context;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using API.Extensions;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenreVectorsController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<GenreVectorsController> logger;

    public GenreVectorsController(EndlessContext context, ILogger<GenreVectorsController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult<List<GenreDto>>> CreateGenreVector(GenreVectorCreateCommand createDto)
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

        logger.LogInformation("Created new genres {Count}",
            newGenres.Count);

        return Ok(newGenres.GetGenreResponsesDto());
    }
}