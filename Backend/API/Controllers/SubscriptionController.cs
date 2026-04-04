using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Backend.API.Data.Components;
using Microsoft.AspNetCore.Mvc;
using Backend.API.Data.Context;
using Backend.API.Data.Models;
using Backend.API.Managers;
using Backend.API.Dtos;

namespace Backend.API.EndPoints.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<SubscriptionController> logger;

    public SubscriptionController(EndlessContext context, ILogger<SubscriptionController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost("domain/{DomainId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<DomainResponseDto>> Subscription(Guid DomainId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var domain = await context.Domains
            .Select(domain => new
            {
                d = domain,
                dResponse = new DomainResponseDto(
                    domain.Id, domain.Name, "@" + domain.Slug,
                    domain.Description ?? "", domain.CreatedDate,
                    domain.AvatarPhotoUrl, domain.Subscribers.Count,
                    domain.Contents.Count, domain.Owners.Count,
                    domain.TotalLikes, domain.TotalViews)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(domain => domain.d.Id == DomainId);

        if (currentUser is null)
            return NotFound("User not found");
        if (domain is null || domain is null)
            return NotFound("Domain not found");

        DomainSubscription domainSubscription = new()
        {
            SubscriberId = currentUserId,
            DomainId = DomainId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.DomainSubscriptions.Add(domainSubscription);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} subscriped domain {DomainId}",
          currentUserId, DomainId);

        return Created($"api/subscription/user/{currentUserId}/domain/{DomainId}",
            domain.dResponse);
    }

    [HttpGet("user/{UserId}/domain/{DomainId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetSubscribedDomains(Guid UserId, Guid DomainId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("domain/{DomainId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserSubscribedDomains(Guid DomainId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("domain/{DomainId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReSubscription(Guid DomainId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        DomainSubscription? domainSubscription = await context.DomainSubscriptions
            .Include(domainSubscription => domainSubscription.Subscriber)
            .Include(domainSubscription => domainSubscription.Domain)
            .FirstOrDefaultAsync(domainSubscription =>
                domainSubscription.SubscriberId == currentUserId &&
                domainSubscription.DomainId == DomainId);

        if (domainSubscription is null)
            return NotFound("Subscriped Domain not found");

        context.DomainSubscriptions.Remove(domainSubscription);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re subscriped domain {DomainId}",
          currentUserId, DomainId);

        return NoContent();
    }
}