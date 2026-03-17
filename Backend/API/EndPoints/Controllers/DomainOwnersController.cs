using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DomainOwnersController : ControllerBase
{
    private readonly EndlessContext context;

    public DomainOwnersController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpGet("domain/{DomainId}")]
    public async Task<IActionResult> GetDomainOwners(Guid DomainId)
    {
        List<DomainOwnerResponseDto> domainOwners = await context.DomainOwners
        .Where(owner => owner.DomainId == DomainId)
        .Select(owner => new DomainOwnerResponseDto(
            owner.OwnerId, owner.DomainId, owner.OwnedDate,
            owner.OwnerRole)).AsNoTracking().ToListAsync();

        return Ok(domainOwners);
    }

    [HttpPost("domain/{DomainId}/user/{UserId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> CreateDomainOwner(Guid DomainId, Guid UserId)
    {
        return Ok();
    }
}