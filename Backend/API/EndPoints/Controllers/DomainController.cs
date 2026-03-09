using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Extensions;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainController : ControllerBase
{
    private readonly EndlessContext context;
    public DomainController(EndlessContext context)
    {
        this.context = context;
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

        bool NameExists = await context.Domains.AnyAsync(d => d.Name == createDto.Name || d.Slug == slug);

        if (NameExists)
            return BadRequest("Domain name or slug already exists");

        Domain domain = new()
        {
            Slug = slug,
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

        domain.Owners.Add(domainOwner);

        context.Domains.Add(domain);

        await context.SaveChangesAsync();

        DomainResponseDto responseDto = new(domain.Id,
            domain.Name, "@" + slug, domain.CreatedDate);

        return Ok(responseDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetDomains() //Searching
    {
        List<DomainResponseDto> domains = await context.Domains.Select(domain => new DomainResponseDto(domain.Id,
                  domain.Name, "@" + domain.Slug, domain.CreatedDate)).AsNoTracking().ToListAsync();

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

        DomainResponseDto responseDto = new(domain.Id,
                  domain.Name, "@" + slug, domain.CreatedDate);

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

        return NoContent();
    }
}