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
public class SubscriptionController : ControllerBase
{
    private readonly EndlessContext context;

    public SubscriptionController(EndlessContext context)
    {
        this.context = context;
    }

    [HttpPost("domain/{DomainId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<DomainResponseDto>> Subscrioption(Guid DomainId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var domain = await context.Domains
            .Select(domain => new {
                d = domain, dResponse = new DomainResponseDto(
                    domain.Id, domain.Name, "@" + domain.Slug,
                    domain.Description ?? "", domain.CreatedDate,
                    domain.AvatarPhotoUrl, domain.Subscribers.Count,
                    domain.Contents.Count, domain.Owners.Count,
                    domain.TotalLikes, domain.TotalViews)})
            .AsNoTracking()
            .FirstOrDefaultAsync(domain => domain.d.Id == DomainId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (domain is null || domain is null)
            return BadRequest("Domain not found");

        DomainSubscription domainSubscription = new()
        {
            SubscriberId = currentUserId,
            DomainId = DomainId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.DomainSubscriptions.Add(domainSubscription);

        await context.SaveChangesAsync();

        return Ok(domain.dResponse);
    }

    [HttpDelete("domain/{DomainId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReSubscrioption(Guid DomainId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DomainSubscription? domainSubscription = await context.DomainSubscriptions
            .Include(domainSubscription => domainSubscription.Subscriber)
            .Include(domainSubscription => domainSubscription.Domain)
            .FirstOrDefaultAsync(domainSubscription =>
                domainSubscription.SubscriberId == currentUserId &&
                domainSubscription.DomainId == DomainId);

        if (domainSubscription is null)
            return BadRequest("Subscriped Domain not found");

        context.DomainSubscriptions.Remove(domainSubscription);

        await context.SaveChangesAsync();

        return NoContent();
    }
}