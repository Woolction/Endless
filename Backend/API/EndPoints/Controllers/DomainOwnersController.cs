using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Extensions;
using Backend.API.Data.Models;
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

    [HttpGet("user/{UserId}/domain/{DomainId}")]
    public async Task<ActionResult<DomainOwnerResponseDto>> GetDomainOwnerById(Guid UserId, Guid DomainId)
    {
        DomainOwnerResponseDto? domainOwner = await context.DomainOwners
            .Select(owner => new DomainOwnerResponseDto(
                owner.OwnerId, owner.DomainId, owner.OwnedDate,
                owner.OwnerRole.ToString()))
            .AsNoTracking()
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == UserId && owner.DomainId == DomainId);

        if (domainOwner == null)
            return NotFound();

        return Ok(domainOwner);
    }

    [HttpGet("domain/{DomainId}")]
    public async Task<ActionResult<DomainOwnerResponseDto[]>> GetDomainOwnersByDomain(Guid DomainId)
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
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (currentOwner is null)
            return Forbid("You not owner the domain");
        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
            return Forbid("You do not have sufficient rights");

        User? user = await context.Users.FindAsync(UserId);

        if (user is null)
            return NotFound("User not found");

        DomainOwner domainOwner = new()
        {
            DomainId = DomainId,
            OwnerId = UserId,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = ownerRole
        };

        DomainSubscription domainSubscription = new()
        {
            DomainId = DomainId,
            SubscriberId = UserId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.DomainSubscriptions.Add(domainSubscription);
        context.DomainOwners.Add(domainOwner);

        await context.SaveChangesAsync();

        return Created($"api/domainowners/user/{UserId}/domain/{DomainId}",
            context.Domains.Select(domain => new DomainResponseDto(
                domain.Id, domain.Name, "@" + domain.Slug,
                domain.Description ?? "", domain.CreatedDate,
                domain.AvatarPhotoUrl, domain.Subscribers.Count,
                domain.Contents.Count, domain.Owners.Count,
                domain.TotalLikes, domain.TotalViews))
            .FirstAsync(domain => domain.Id == DomainId));
    }

    [Authorize(Policy = nameof(UserRole.Creator))]
    [HttpDelete("domain/{DomainId}/owner/{OwnerId}")]
    public async Task<IActionResult> DeleteOwner(Guid DomainId, Guid OwnerId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DomainOwner? currentOwner = await context.DomainOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.DomainId == DomainId);

        if (currentOwner is null)
            return Forbid("You not owner the domain");
        if (currentOwner.OwnerRole != DomainOwnerRole.Admin)
            return Forbid("You do not have sufficient rights");

        DomainOwner? owner = await context.DomainOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == OwnerId &&
                owner.DomainId == DomainId);

        if (owner == null)
            return NotFound("Owner not found");

        context.DomainOwners.Remove(owner);

        await context.SaveChangesAsync();

        return NoContent();
    }
}