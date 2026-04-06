using Application.Dtos.Channels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Domain.Components;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Context;
using Domain.Entities;
using Infrastructure.Managers;

namespace API.Controllers;

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

    [HttpPost("Channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ChannelDto>> Subscription(Guid ChannelId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var Channel = await context.Channels
            .Select(Channel => new
            {
                d = Channel,
                dResponse = new ChannelDto(
                    Channel.Id, Channel.Name, "@" + Channel.Slug,
                    Channel.Description ?? "", Channel.CreatedDate,
                    Channel.AvatarPhotoUrl, Channel.Subscribers.Count,
                    Channel.Contents.Count, Channel.Owners.Count,
                    Channel.TotalLikes, Channel.TotalViews)
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(Channel => Channel.d.Id == ChannelId);

        if (currentUser is null)
            return NotFound("User not found");
        if (Channel is null || Channel is null)
            return NotFound("Channel not found");

        ChannelSubscription ChannelSubscription = new()
        {
            SubscriberId = currentUserId,
            ChannelId = ChannelId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.ChannelSubscriptions.Add(ChannelSubscription);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} subscriped Channel {ChannelId}",
          currentUserId, ChannelId);

        return Created($"api/subscription/user/{currentUserId}/Channel/{ChannelId}",
            Channel.dResponse);
    }

    [HttpGet("user/{UserId}/Channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetSubscribedChannels(Guid UserId, Guid ChannelId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("Channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserSubscribedChannels(Guid ChannelId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("Channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReSubscription(Guid ChannelId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        ChannelSubscription? ChannelSubscription = await context.ChannelSubscriptions
            .Include(ChannelSubscription => ChannelSubscription.Subscriber)
            .Include(ChannelSubscription => ChannelSubscription.Channel)
            .FirstOrDefaultAsync(ChannelSubscription =>
                ChannelSubscription.SubscriberId == currentUserId &&
                ChannelSubscription.ChannelId == ChannelId);

        if (ChannelSubscription is null)
            return NotFound("Subscriped Channel not found");

        context.ChannelSubscriptions.Remove(ChannelSubscription);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re subscriped Channel {ChannelId}",
          currentUserId, ChannelId);

        return NoContent();
    }
}