using Microsoft.AspNetCore.Authorization;
using Application.Features.Channels.Dtos;
using Microsoft.EntityFrameworkCore;
using Application.Features.Images;
using Application.Interfaces.Db;
using Microsoft.AspNetCore.Mvc;
using Application.Utilities;
using Domain.Common.Enums;
using Domain.Entities;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionController : ControllerBase
{
    private readonly IAppDbContext context;

    private readonly ILogger<SubscriptionController> logger;

    public SubscriptionController(IAppDbContext context, ILogger<SubscriptionController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpPost("channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ChannelDto>> Subscription(Guid ChannelId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? currentUser = await context.Users.FindAsync(currentUserId);
        var channel = await context.Channels
            .Where(channel => channel.Id == ChannelId)
            .Select(channel => new ChannelDto(
                    channel.Id, channel.Name, "@" + channel.Slug,
                    channel.Description ?? "", channel.CreatedDate,
                    new ImageDto(
                        new ImageVariantsDto(
                            channel.ChannelMeta.Image.BaseUrl,
                            channel.ChannelMeta.Image.Variants
                                .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                                .ToList()),
                        channel.ChannelMeta.Image.R,
                        channel.ChannelMeta.Image.G,
                        channel.ChannelMeta.Image.B),
                    channel.Subscribers.Count + 1,
                    0, 0, channel.TotalLikes, channel.TotalViews))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (currentUser is null)
            return NotFound("User not found");
        if (channel is null)
            return NotFound("channel not found");

        ChannelSubscription ChannelSubscription = new()
        {
            SubscriberId = currentUserId,
            ChannelId = ChannelId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.ChannelSubscriptions.Add(ChannelSubscription);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} subscriped channel {ChannelId}",
          currentUserId, ChannelId);

        return Created($"api/subscription/user/{currentUserId}/channel/{ChannelId}",
            channel);
    }

    [HttpGet("user/{UserId}/channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetSubscribedChannels(Guid UserId, Guid ChannelId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpGet("channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult> GetCurrentUserSubscribedChannels(Guid ChannelId)
    {
        return NotFound("Dont released this end point");
    }

    [HttpDelete("channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<IActionResult> ReSubscription(Guid ChannelId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        ChannelSubscription? ChannelSubscription = await context.ChannelSubscriptions
            .FirstOrDefaultAsync(ChannelSubscription =>
                ChannelSubscription.SubscriberId == currentUserId &&
                ChannelSubscription.ChannelId == ChannelId);

        if (ChannelSubscription is null)
            return NotFound("Subscriped channel not found");

        context.ChannelSubscriptions.Remove(ChannelSubscription);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} re subscriped channel {ChannelId}",
          currentUserId, ChannelId);

        return NoContent();
    }
}