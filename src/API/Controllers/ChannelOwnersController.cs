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
public class ChannelOwnersController : ControllerBase
{
    private readonly IAppDbContext context;

    private readonly ILogger<ChannelOwnersController> logger;

    public ChannelOwnersController(IAppDbContext context, ILogger<ChannelOwnersController> logger)
    {
        this.context = context;

        this.logger = logger;
    }

    [HttpGet("user/{UserId}/Channel/{ChannelId}")]
    public async Task<ActionResult<ChannelOwnerDto>> GetChannelOwnerById(Guid UserId, Guid ChannelId)
    {
        ChannelOwnerDto? ChannelOwner = await context.ChannelOwners
            .Where(owner =>
                owner.OwnerId == UserId &&
                owner.ChannelId == ChannelId)
            .Select(owner => new ChannelOwnerDto(
                owner.OwnerId, owner.ChannelId, owner.OwnedDate,
                owner.OwnerRole.ToString()))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (ChannelOwner == null)
            return NotFound();

        logger.LogInformation("Returned owner {OwnerId} Channel {ChannelId}",
            UserId, ChannelId);

        return Ok(ChannelOwner);
    }

    [HttpGet("Channel/{ChannelId}")]
    public async Task<ActionResult<ChannelOwnerDto[]>> GetChannelOwnersByChannel(Guid ChannelId)
    {
        ChannelOwnerDto[] ChannelOwners = await context.ChannelOwners
            .Where(owner => owner.ChannelId == ChannelId)
            .Select(owner => new ChannelOwnerDto(
                owner.OwnerId, owner.ChannelId, owner.OwnedDate,
                owner.OwnerRole.ToString())).AsNoTracking().ToArrayAsync();

        logger.LogInformation("Returned owners {Count} for Channel {ChannelId}",
            ChannelOwners.Length, ChannelId);

        return Ok(ChannelOwners);
    }

    [HttpPost("Channel/{ChannelId}/user/{UserId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ChannelDto>> CreateChannelOwner(Guid ChannelId, Guid UserId, [FromBody] ChannelOwnerRole ownerRole)
    {
        Guid currentUserId = this.GetIDFromClaim();

        ChannelOwner? currentOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.ChannelId == ChannelId);

        if (currentOwner is null)
        {
            logger.LogWarning("User {UserId} tried to create owner for Channel {ChannelId} without permission",
                currentUserId, ChannelId);
            return Forbid("You not owner the Channel");
        }
        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("User {UserId} tried to create owner for Channel {ChannelId} without permission",
                currentUserId, ChannelId);
            return Forbid("You do not have sufficient rights");
        }

        User? user = await context.Users.FindAsync(UserId);

        if (user is null)
            return NotFound("User not found");

        ChannelOwner ChannelOwner = new()
        {
            ChannelId = ChannelId,
            OwnerId = UserId,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = ownerRole
        };

        ChannelSubscription ChannelSubscription = new()
        {
            ChannelId = ChannelId,
            SubscriberId = UserId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.ChannelSubscriptions.Add(ChannelSubscription);
        context.ChannelOwners.Add(ChannelOwner);

        await context.SaveChangesAsync();

        logger.LogInformation("User {UserId} create owner {OwnerId} for Channel {ChannelId}",
            currentUserId, UserId, ChannelId);

        return Created($"api/ChannelOwners/user/{UserId}/Channel/{ChannelId}",
            await context.Channels
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
                channel.Subscribers.Count, channel.Contents.Count,
                channel.Owners.Count, channel.TotalLikes, channel.TotalViews))
                .AsNoTracking()
                .FirstAsync());
    }

    [Authorize(Policy = nameof(UserRole.Creator))]
    [HttpDelete("Channel/{ChannelId}/owner/{OwnerId}")]
    public async Task<IActionResult> DeleteOwner(Guid ChannelId, Guid OwnerId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        ChannelOwner? currentOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.ChannelId == ChannelId);

        ChannelOwner? owner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == OwnerId &&
                owner.ChannelId == ChannelId);

        if (owner == null)
            return NotFound("Owner not found");

        if (currentOwner is null)
        {
            logger.LogWarning("User {UserId} tried to delete owner {OwnerId} for Channel {ChannelId} without permission",
               currentUserId, OwnerId, ChannelId);
            return Forbid("You not owner the Channel");
        }
        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("User {UserId} tried to delete owner {OwnerId} for Channel {ChannelId} without permission",
               currentUserId, OwnerId, ChannelId);
            return Forbid("You do not have sufficient rights");
        }

        context.ChannelOwners.Remove(owner);

        await context.SaveChangesAsync();

        logger.LogInformation("Owner {OwnerId} for Channel {ChannelId} deleted",
            OwnerId, ChannelId);

        return NoContent();
    }
}