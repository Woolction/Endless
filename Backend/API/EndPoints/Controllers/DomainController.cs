
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Context.Migrations;
using Backend.API.Data.Models;
using Backend.API.Dtos;
using Backend.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.API.EndPoints.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DomainController : ControllerBase
{
    private readonly EndlessContext context;
    public DomainController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Creator))]
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
            User = user,
            Domain = domain,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = DomainOwnerRole.Admin
        };

        domain.Owners.Add(domainOwner);

        context.Domains.Add(domain);

        await context.SaveChangesAsync();

        DomainResponseDto responseDto = new(
            domain.Name, domain.CreatedDate);

        return Ok(responseDto);
    }
}