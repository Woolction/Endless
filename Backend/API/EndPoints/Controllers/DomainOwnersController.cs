using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Dtos;
using Backend.API.Extensions;
using Backend.API.Data.Models;

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
    public async Task<ActionResult<DomainOwnerResponseDto[]>> GetDomainOwners(Guid DomainId)
    {
        DomainOwnerResponseDto[] domainOwners = await context.DomainOwners
            .Where(owner => owner.DomainId == DomainId)
            .Select(owner => new DomainOwnerResponseDto(
                owner.OwnerId, owner.DomainId, owner.OwnedDate,
                owner.OwnerRole.ToString())).AsNoTracking().ToArrayAsync();

        return Ok(domainOwners);
    }

    [HttpPost("domain/{DomainId}/user/{UserId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<DomainResponseDto>> CreateDomainOwner(Guid DomainId, Guid UserId, [FromBody] DomainOwnerRole ownerRole)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DomainOwner? currentOwner = await context.DomainOwners
            .Include(owner => owner.Domain)
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (currentOwner is null)
            return BadRequest("You not owner the domain");
        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
            return BadRequest("You do not have sufficient rights");

        User? user = await context.Users.FindAsync(UserId);

        if (user is null)
            return BadRequest("User not found");

        user.OwnedDomainsCount++;
        currentOwner.Domain!.OwnersCount++;

        DomainOwner domainOwner = new()
        {
            DomainId = DomainId,
            OwnerId = UserId,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = ownerRole
        };

        context.DomainOwners.Add(domainOwner);

        await context.SaveChangesAsync();

        return Ok(currentOwner.Domain.GetDomainResponseDto());
    }

    [Authorize(Policy = nameof(UserRole.Creator))]
    [HttpDelete("domain/{DomainId}/owner/{OwnerId}")]
    public async Task<IActionResult> DeleteOwner(Guid DomainId, Guid OwnerId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DomainOwner? currentOwner = await context.DomainOwners
            .Include(owner => owner.Domain)
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (currentOwner is null)
            return BadRequest("You not owner the domain");
        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
            return BadRequest("You do not have sufficient rights");

        DomainOwner? owner = await context.DomainOwners
            .Include(owner => owner.Owner)
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == OwnerId &&
                owner.DomainId == DomainId);

        if (owner == null)
            return BadRequest("Owner not found");

        owner.Owner!.OwnedDomainsCount--;
        currentOwner.Domain!.OwnersCount--;

        context.DomainOwners.Remove(owner);

        await context.SaveChangesAsync();

        return NoContent();
    }
}