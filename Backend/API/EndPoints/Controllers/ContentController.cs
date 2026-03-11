using System.Reflection.Metadata.Ecma335;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Dtos;
using Backend.API.Extensions;
using Backend.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.EndPoints.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly IRecommendationService recommendation;

    public ContentController(EndlessContext context, IRecommendationService recommendation)
    {
        this.context = context;

        this.recommendation = recommendation;
    }

    [HttpGet]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> GetContentForRecommendation(ContentRecoRequestDto requestDto)
    {
        Guid id = this.GetIDFromClaim();

        User? currentUser = await context.Users
            .Include(u => u.Vectors)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (currentUser is null)
            return BadRequest("User Not found");

        double r = Random.Shared.NextDouble();

        var candidates = await context.Contents
            .AsNoTracking()
            .Where(c => c.RandomKey >= r)
            .Include(c => c.Vectors)
            .Include(c => c.VideoMeta)
            .OrderBy(c => c.RandomKey)
            .Take(300)
            .ToListAsync();

        if (candidates.Count < 300)
        {
            var extra = await context.Contents
                .AsNoTracking()
                .Where(c => c.RandomKey < r)
                .Include(c => c.Vectors)
                .Include(c => c.VideoMeta)
                .OrderBy(c => c.RandomKey)
                .Take(300 - candidates.Count)
                .ToListAsync();

            candidates.AddRange(extra);
        }

        ContentRecoScoreDto[] recommended = [];

        if (requestDto.LastRecommendScore is not null)
        {
            recommended = candidates
                .Select(c => new ContentRecoScoreDto(
                    c, recommendation.Recommend(currentUser, c)))
                .Where(c =>
                   c.Score < requestDto.LastRecommendScore)
                .OrderByDescending(x => x.Score)
                .Take(16)
                .ToArray();
        }
        else
        {
            recommended = candidates
                .Select(c => new ContentRecoScoreDto(
                    c, recommendation.Recommend(currentUser, c)))
                .OrderByDescending(x => x.Score)
                .Take(16)
                .ToArray();
        }

        var recommendedIds = recommended.Select(x => x.Content.Id).ToHashSet();

        var random = await context.Contents
            .Where(x => !recommendedIds.Contains(x.Id))
            .OrderBy(x => x.RandomKey >= r)
            .Take(4)
            .ToListAsync();

        var LastRecommended = recommended.LastOrDefault();
        float? lastScore = null;

        if (LastRecommended is not null)
            lastScore = LastRecommended.Score;

        var combined = recommended
            .Select(x => new ContentRecoDto(
                x.Content.Title, x.Content.Slug,
                x.Content.Description, x.Content.CreatedDate,
                x.Content.ContentType, Random.Shared.NextDouble()))
            .Concat(random.Select(c => new ContentRecoDto(
                c.Title, c.Slug,
                c.Description, c.CreatedDate,
                c.ContentType, Random.Shared.NextDouble())))
            .OrderBy(x => x.RandomKey)
            .ToArray();

        return Ok(new ContentRecoResponseDto(combined, lastScore));
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetContentForName()
    {
        return Ok();
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> CreateContent()
    {
        return Ok();
    }

    [HttpPut]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> UpdateContent()
    {
        return Ok();
    }

    [HttpDelete]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteContent()
    {
        return NoContent();
    }
}