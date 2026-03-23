using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Dtos;
using Npgsql;
using Backend.API.Services;
using System.Runtime.InteropServices;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<DomainController> logger;
    private readonly IR2Service r2Service;
    
    public DomainController(EndlessContext context, ILogger<DomainController> logger, IR2Service r2Service)
    {
        this.context = context;

        this.logger = logger;
        this.r2Service = r2Service;
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<DomainResponseDto>> CreateDomain(DomainCreateDto createDto)
    {
        Guid id = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(id);

        if (user is null)
            return BadRequest("User not found");

        string slug = createDto.Name.GenerateSlug();

        Domain domain = new()
        {
            Slug = slug,
            Name = createDto.Name,
            CreatedDate = DateTime.UtcNow,
        };

        if (createDto.AvatarPhoto is not null && createDto.AvatarPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(createDto.AvatarPhoto, "Images", ".jpeg");

            string photoUrl = await r2Service.SaveImage(photoPath);

            domain.AvatarPhotoUrl = photoUrl;

            System.IO.File.Delete(photoPath);
        }

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
            Subscriber = user,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.DomainSubscriptions.Add(domainSubscription);
        context.DomainOwners.Add(domainOwner);
        context.Domains.Add(domain);

        try
        {
            await context.SaveChangesAsync();

            return Ok(domain.GetDomainResponseDto());
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return BadRequest("Domain name or slug already exists");

            throw;
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<DomainSearchResponseDto>> GetDomainsForName([FromQuery] SearchRequestDto requestDto)
    {
        IQueryable<Domain> query = context.Domains.AsQueryable();

        if (requestDto.LastSearch is not null)
        {
            query = query.Where(domain =>
                EF.Functions.ILike(domain.Name, $"%{requestDto.Name}%") == requestDto.LastSearch.LastLiked &&
                EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name) < requestDto.LastSearch.LastSimilarity &&
                EF.Functions.FuzzyStringMatchLevenshtein(domain.Name, requestDto.Name) >= requestDto.LastSearch.LastLevenshit);
        }
        else
        {
            query = query.Where(domain =>
                EF.Functions.ILike(domain.Name, $"%{requestDto.Name}%") ||
                EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name) > 0.2f ||
                EF.Functions.FuzzyStringMatchLevenshtein(domain.Name, requestDto.Name) <= 3);
        }

        var domains = await query
            .OrderByDescending(domain => EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name))
            .Take(20)
            .Select(domain => new
            {
                Domain = domain.GetDomainResponseDto(),
                LastLiked = EF.Functions.ILike(domain.Name, $"%{requestDto.Name}%"),
                LastSimilarity = EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name),
                LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(domain.Name, requestDto.Name)
            })
            .AsNoTracking().ToArrayAsync();

        var lastResponse = domains.LastOrDefault();

        DomainResponseDto[] domainResponses = domains.Select(domain => domain.Domain).ToArray();

        return Ok(new DomainSearchResponseDto(
            domainResponses, lastResponse == null ? null : GetSearchDto(
                lastResponse.LastLiked, lastResponse.LastSimilarity, lastResponse.LastLevenshit)));
    }

    [HttpGet]
    public async Task<ActionResult<DomainResponseDto[]>> GetDomains()
    {
        DomainResponseDto[] domains = await context.Domains
            .Select(domain => domain.GetDomainResponseDto())
            .AsNoTracking().ToArrayAsync();

        return Ok(domains);
    }

    [HttpPut("{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<DomainResponseDto>> UpdateDomain(Guid DomainId, DomainUpdateDto updateDto)
    {
        Domain? domain = await context.Domains.FindAsync(DomainId);

        if (domain is null)
            return BadRequest("Domain not found");

        Guid currentUserId = this.GetIDFromClaim();

        DomainOwner? currentOwner = await context.DomainOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (currentOwner == null)
            return Forbid("You doesn't owner the Domain");

        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
            return BadRequest("You do not have sufficient rights");

        if (!string.IsNullOrEmpty(updateDto.Description))
            domain.Description = updateDto.Description;

        string slug = updateDto.Name.GenerateSlug();

        if (await context.Domains.AnyAsync(domain => domain.Name == updateDto.Name || domain.Slug == slug))
            return BadRequest($"Domain whit name {updateDto.Name} hasted+");

        domain.Name = updateDto.Name;
        domain.Slug = slug;
            
        await context.SaveChangesAsync();

        return Ok(domain.GetDomainResponseDto());
    }

    [HttpDelete("{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteDomain(Guid DomainId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DomainOwner? currentOwner = await context.DomainOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (currentOwner == null)
            return Forbid("You doesn't owner the Domain");

        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
            return BadRequest("You do not have sufficient rights");

        Domain? domain = await context.Domains.FindAsync(DomainId);

        if (domain == null)
            return BadRequest("Domain not found");

        context.Domains.Remove(domain);

        await context.SaveChangesAsync();


        return NoContent();
    }

    private SearchDto GetSearchDto(bool IsLastLiked, double LastSimilarity, int LastLevenshit)
    {
        return new()
        {
            LastLiked = IsLastLiked,
            LastSimilarity = LastSimilarity,
            LastLevenshit = LastLevenshit
        };
    }
}