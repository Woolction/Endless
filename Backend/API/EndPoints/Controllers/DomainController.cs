using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Services;
using Backend.API.Dtos;
using Npgsql;


namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<DomainController> logger;
    private readonly IR2Service r2Service;
    
    public DomainController(EndlessContext context, IR2Service r2Service, ILogger<DomainController> logger)
    {
        this.context = context;

        this.r2Service = r2Service;
        this.logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<DomainResponseDto>> CreateDomain(DomainCreateDto createDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return NotFound("User not found");

        string slug = createDto.Name.GenerateSlug();

        Domain domain = new()
        {
            Slug = slug,
            Name = createDto.Name,
            CreatedDate = DateTime.UtcNow,
        };

        if (createDto.AvatarPhoto is not null && createDto.AvatarPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(
                createDto.AvatarPhoto, "Images", ".jpeg");

            string photoUrl = await r2Service.SaveImage(photoPath);

            domain.AvatarPhotoUrl = photoUrl;

            System.IO.File.Delete(photoPath);
        }

        DomainOwner domainOwner = new()
        {
            OwnerId = currentUserId,
            Domain = domain,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = DomainOwnerRole.Admin
        };

        DomainSubscription domainSubscription = new()
        {
            Domain = domain,
            SubscriberId = currentUserId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.DomainSubscriptions.Add(domainSubscription);
        context.DomainOwners.Add(domainOwner);
        context.Domains.Add(domain);

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation("Domain {DomainId} created with slug {Slug}",
                domain.Id, slug);

            return Created($"api/domain/{domain.Id}", new DomainResponseDto(
                domain.Id,
                domain.Name,
                "@" + domain.Slug,
                domain.Description ?? "",
                domain.CreatedDate,
                domain.AvatarPhotoUrl,
                1, 0, 1,
                domain.TotalLikes,
                domain.TotalViews));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while creating domain for user {UserId}", currentUserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Conflict("Domain name already exists");

            throw;
        }
    }

    [HttpGet("{DomainId}")]
    public async Task<ActionResult<DomainResponseDto>> GetDomainById(Guid DomainId)
    {
        DomainResponseDto? domain = await context.Domains
            .Select(domain => new DomainResponseDto(
                domain.Id, domain.Name, "@" + domain.Slug,
                domain.Description ?? "", domain.CreatedDate,
                domain.AvatarPhotoUrl, domain.Subscribers.Count,
                domain.Contents.Count, domain.Owners.Count,
                domain.TotalLikes, domain.TotalViews))
            .AsNoTracking()
            .FirstOrDefaultAsync(domain => domain.Id == DomainId);

        if (domain == null)
            return NotFound("Domain not found");

        logger.LogInformation("Getted domain {DomainId}",
            DomainId);

        return Ok(domain);
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
                Domain = new DomainResponseDto(
                    domain.Id, domain.Name, "@" + domain.Slug,
                    domain.Description ?? "", domain.CreatedDate,
                    domain.AvatarPhotoUrl, domain.Subscribers.Count,
                    domain.Contents.Count, domain.Owners.Count,
                    domain.TotalLikes, domain.TotalViews),
                LastLiked = EF.Functions.ILike(domain.Name, $"%{requestDto.Name}%"),
                LastSimilarity = EF.Functions.TrigramsSimilarity(domain.Name, requestDto.Name),
                LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(domain.Name, requestDto.Name)
            })
            .AsNoTracking().ToArrayAsync();

        var lastResponse = domains.LastOrDefault();

        DomainResponseDto[] domainResponses = domains.Select(domain => domain.Domain).ToArray();

        logger.LogInformation("Search returned domains {Count} results for {Query}",
            domains.Length, requestDto.Name);

        return Ok(new DomainSearchResponseDto(
            domainResponses, lastResponse == null ? null : GetSearchDto(
                lastResponse.LastLiked, lastResponse.LastSimilarity, lastResponse.LastLevenshit)));
    }

    [HttpGet]
    public async Task<ActionResult<DomainResponseDto[]>> GetDomains()
    {
        DomainResponseDto[] domains = await context.Domains
            .Select(domain => new DomainResponseDto(
                domain.Id, domain.Name, "@" + domain.Slug,
                domain.Description ?? "", domain.CreatedDate,
                domain.AvatarPhotoUrl, domain.Subscribers.Count,
                domain.Contents.Count, domain.Owners.Count,
                domain.TotalLikes, domain.TotalViews))
            .AsNoTracking().ToArrayAsync();

        logger.LogInformation("Returned {Count} domains", domains.Length);

        return Ok(domains);
    }

    [HttpPut("{DomainId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<DomainResponseDto>> UpdateDomain(Guid DomainId, DomainUpdateDto updateDto)
    {
        var domain = await context.Domains
            .Where(domain => domain.Id == DomainId)
            .Select(domain => new {
                d = domain,
                SubscribersCount = domain.Subscribers.Count,
                ContentsCount = domain.Contents.Count,
                OwnersCount = domain.Owners.Count})// For mapping
            .FirstOrDefaultAsync();

        if (domain is null || domain.d is null)
            return NotFound("Domain not found");

        Guid currentUserId = this.GetIDFromClaim();

        DomainOwner? currentOwner = await context.DomainOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (currentOwner == null)
        {
            logger.LogWarning("User {UserId} tried to update domain without permission",
                currentUserId);
            return Forbid("You doesn't owner the Domain");
        }

        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
        {
            logger.LogWarning("User {UserId} tried to update domain without permission",
                currentUserId);
            return Forbid("You do not have sufficient rights");
        }

        if (!string.IsNullOrEmpty(updateDto.Description))
            domain.d.Description = updateDto.Description;

        string slug = updateDto.Name.GenerateSlug();

        if (await context.Domains.AnyAsync(domain => domain.Name == updateDto.Name || domain.Slug == slug))
            return Conflict($"Domain whit name {updateDto.Name} hasted");

        string oldName = domain.d.Name;

        //Updating
        domain.d.Name = updateDto.Name;
        domain.d.Slug = slug;

        DomainResponseDto responseDto = new(
                domain.d.Id, domain.d.Name, "@" + domain.d.Slug,
                domain.d.Description ?? "", domain.d.CreatedDate,
                domain.d.AvatarPhotoUrl, domain.SubscribersCount,
                domain.ContentsCount, domain.OwnersCount,
                domain.d.TotalLikes, domain.d.TotalViews);

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Domain {DomainId} updated successfully. Changed the name from: {OldName} to: {NewName}",
                DomainId, oldName, updateDto.Name);

            return Ok(responseDto);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while creating domain for user {UserId}", currentUserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Conflict("Domain name already exists");

            throw;
        }
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
        {
            logger.LogWarning("User {UserId} tried to delete domain {DomainId} without permission",
                currentUserId, DomainId);

            return Forbid("You doesn't owner the Domain");
        }

        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
        {
            logger.LogWarning("Delete denied for user {UserId} on domain {DomainId}",
                currentUserId, DomainId);

            return Forbid("You do not have sufficient rights");
        }

        Domain? domain = await context.Domains.FindAsync(DomainId);

        if (domain == null)
            return NotFound("Domain not found");

        context.Domains.Remove(domain);

        await context.SaveChangesAsync();

        logger.LogInformation("Domain {DomainId} deleted", DomainId);

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