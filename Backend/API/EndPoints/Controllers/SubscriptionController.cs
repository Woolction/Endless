using Backend.API.Data.Components;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Dtos;
using Backend.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        Domain? domain = await context.Domains.FindAsync(DomainId);

        if (currentUser is null)
            return BadRequest("User not found");
        if (domain is null)
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

        return Ok(domain.GetDomainResponseDto());
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