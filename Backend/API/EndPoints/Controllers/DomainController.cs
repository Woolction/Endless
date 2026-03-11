using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Dtos;
using Npgsql;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<DomainController> logger;
    
    public DomainController(EndlessContext context, ILogger<DomainController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> CreateDomain(DomainCreateDto createDto)
    {
        Guid id = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(id);

        if (user is null)
            return BadRequest("User not found");

        string slug = createDto.Name.GenerateSlug();

        Domain domain = new()
        {
            Slug = slug,
            OwnersCount = 1,
            SubscribersCount = 1,
            Name = createDto.Name,
            CreatedDate = DateTime.UtcNow,
        };

        DomainOwner domainOwner = new()
        {
            Owner = user,
            Domain = domain,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = DomainOwnerRole.Admin
        };

        DomainSubscription domainSubscription = new()
        {
            Domain = domain,
            SubscribedUser = user,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        user.OwnedDomainsCount++;
        user.DomainSubscriptionsCount++;

        context.DomainSubscriptions.Add(domainSubscription);
        context.DomainOwners.Add(domainOwner);
        context.Domains.Add(domain);

        try
        {
            await context.SaveChangesAsync();

            DomainResponseDto responseDto = new(
                domain.Id,
                domain.Name,
                "@" + domain.Slug,
                domain.Description ?? "",
                domain.CreatedDate,
                domain.SubscribersCount,
                domain.OwnersCount,
                domain.TotalLikes, domain.TotalViews);

            return Ok(responseDto);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return BadRequest("Domain name or slug already exists");

            throw;
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> GetDomainsForName(DomainSearchRequestDto requestDto)
    {
        IQueryable<Domain> query = context.Domains.AsQueryable();

        if (requestDto.LastSimilarity is not null)
        {
            query = query.Where(
                domain => EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name) < requestDto.LastSimilarity);
        }
        else
        {
            query = query.Where(domain =>
                EF.Functions.ILike(domain.Name, $"%{requestDto.Name}%") ||
                EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name) > 0.2f ||
                EF.Functions.FuzzyStringMatchLevenshtein(domain.Name, requestDto.Name) <= 3);
        }

        List<DomainSearchResponseDto> domains = await query
            .OrderByDescending(domain => EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name))
            .Take(25)
            .Select(domain => new DomainSearchResponseDto(
                domain.Id,
                domain.Name,
                "@" + domain.Slug,
                domain.Description ?? "",
                domain.CreatedDate,
                domain.SubscribersCount,
                domain.OwnersCount,
                domain.TotalLikes,
                domain.TotalViews,
                EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name)))
            .AsNoTracking().ToListAsync();

        return Ok(domains);
    }

    [HttpGet]
    public async Task<IActionResult> GetDomains()
    {
        List<DomainResponseDto> domains = await context.Domains
            .Select(domain => new DomainResponseDto(
                domain.Id,
                domain.Name,
                "@" + domain.Slug,
                domain.Description ?? "",
                domain.CreatedDate,
                domain.SubscribersCount,
                domain.OwnersCount,
                domain.TotalLikes,
                domain.TotalViews))
            .AsNoTracking().ToListAsync();

        return Ok(domains);
    }

    [HttpPut("{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> UpdateDomain(Guid DomainId, DomainUpdateDto updateDto)
    {
        Domain? domain = await context.Domains.FindAsync(DomainId);

        if (domain is null)
            return BadRequest("Domain not found");

        Guid currentUserId = this.GetIDFromClaim();

        if (!domain.Owners.Any(owner => owner.OwnerId == currentUserId))
            return Forbid("You doesn't owner the Domain");

        if (!string.IsNullOrEmpty(updateDto.Description))
            domain.Description = updateDto.Description;

        string slug = updateDto.Name.GenerateSlug();

        if (!await context.Domains.AnyAsync(domain => domain.Name == updateDto.Name || domain.Slug == slug))
        {
            domain.Name = updateDto.Name;
            domain.Slug = slug;
        }

        await context.SaveChangesAsync();

        DomainResponseDto responseDto = new(
            domain.Id,
            domain.Name,
            "@" + domain.Slug,
            domain.Description ?? "",
            domain.CreatedDate,
            domain.SubscribersCount,
            domain.OwnersCount,
            domain.TotalLikes,
            domain.TotalViews);

        return Ok(responseDto);
    }

    [HttpDelete("{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteDomain(Guid DomainId)
    {
        Guid UserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(UserId);

        if (user is null)
            return BadRequest("User not found");

        if (!user.OwnedDomains.Any(d => d.DomainId == DomainId))
            return Forbid("You doesn't owner the Domain");

        await context.Domains.Where(domain => domain.Id == DomainId).ExecuteDeleteAsync();

        user.DomainSubscriptionsCount--;
        user.OwnedDomainsCount--;

        return NoContent();
    }
}