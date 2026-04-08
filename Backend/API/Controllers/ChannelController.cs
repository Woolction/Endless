using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Application.Commands.Channels;
using Application.Queries.Searchs;
using Domain.Interfaces.Services;
using Application.Dtos.Channels;
using Microsoft.AspNetCore.Mvc;
using Application.Dtos.Searchs;
using Infrastructure.Context;
using Application.Utilities;
using Domain.Common;
using Domain.Entities;
using Npgsql;
using Application.Handlers;
using Application;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ChannelSearchingHandler searchingHandler;
    private readonly ChannelsCreatingHandler channelsCreating;

    private readonly ILogger<ChannelController> logger;
    private readonly IR2Service r2Service;

    public ChannelController(EndlessContext context, ChannelSearchingHandler searchingHandler, ChannelsCreatingHandler channelsCreating, IR2Service r2Service, ILogger<ChannelController> logger)
    {
        this.context = context;
        this.searchingHandler = searchingHandler;
        this.channelsCreating = channelsCreating;

        this.r2Service = r2Service;
        this.logger = logger;
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ChannelDto>> CreateChannel(ChannelCreateCommand createDto)
    {
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user is null)
            return NotFound("User not found");

        string slug = createDto.Name.GenerateSlug();

        Channel channel = new()
        {
            Slug = slug,
            Name = createDto.Name,
            CreatedDate = DateTime.UtcNow,
        };

        if (createDto.AvatarPhoto is not null && createDto.AvatarPhoto.Length != 0)
        {
            string photoPath = await r2Service.SaveFormFileAsync(
                createDto.AvatarPhoto, "Images", ".jpeg");

            string photoUrl = await r2Service.SaveImage(photoPath);

            channel.AvatarPhotoUrl = photoUrl;

            System.IO.File.Delete(photoPath);
        }

        ChannelOwner channelOwner = new()
        {
            OwnerId = currentUserId,
            Channel = channel,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = ChannelOwnerRole.Admin
        };

        ChannelSubscription channelSubscription = new()
        {
            Channel = channel,
            SubscriberId = currentUserId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.ChannelSubscriptions.Add(channelSubscription);
        context.ChannelOwners.Add(channelOwner);
        context.Channels.Add(channel);

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation("Channel {ChannelId} created with slug {Slug}",
                channel.Id, slug);

            return Created($"api/Channel/{channel.Id}", new ChannelDto(
                channel.Id,
                channel.Name,
                "@" + channel.Slug,
                channel.Description ?? "",
                channel.CreatedDate,
                channel.AvatarPhotoUrl,
                1, 0, 1,
                channel.TotalLikes,
                channel.TotalViews));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while creating Channel for user {UserId}", currentUserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Conflict("Channel name already exists");

            throw;
        }
    }

    [HttpPost("many")]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult<ChannelDto[]>> CreateChannels(ChannelsCreateCommand cmd)
    {
        if (cmd.Count < 1)
            return BadRequest($"Count < 1: {cmd.Count}");
            
        Guid currentUserId = this.GetIDFromClaim();

        User? user = await context.Users.FindAsync(currentUserId);

        if (user == null)
            return NotFound("User not found");

        cmd.UserId = currentUserId;

        Result<ChannelDto[]> result = await channelsCreating.Handle(cmd);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                409 => Conflict(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created("", result.Data);
    }

    [HttpGet("{ChannelId}")]
    public async Task<ActionResult<ChannelDto>> GetChannelById(Guid ChannelId)
    {
        ChannelDto? Channel = await context.Channels
            .Where(Channel => Channel.Id == ChannelId)
            .Select(Channel => new ChannelDto(
                Channel.Id, Channel.Name, "@" + Channel.Slug,
                Channel.Description ?? "", Channel.CreatedDate,
                Channel.AvatarPhotoUrl, Channel.Subscribers.Count,
                Channel.Contents.Count, Channel.Owners.Count,
                Channel.TotalLikes, Channel.TotalViews))
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (Channel == null)
            return NotFound("Channel not found");

        logger.LogInformation("Getted Channel {ChannelId}",
            ChannelId);

        return Ok(Channel);
    }

    [HttpGet("search")]
    public async Task<ActionResult<ChannelSearchDto>> GetChannelsByName([FromQuery] SearchQuery query)
    {
        Result<ChannelSearchDto> result = await searchingHandler.Handle(query);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "Unknown error")
            };
        }

        logger.LogInformation("Search returned Channels {Count} results for {Query}",
           result.Data.ChannelsDto.Length, query.Name);

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<ActionResult<ChannelDto[]>> GetChannels()
    {
        ChannelDto[] Channels = await context.Channels
            .Select(Channel => new ChannelDto(
                Channel.Id, Channel.Name, "@" + Channel.Slug,
                Channel.Description ?? "", Channel.CreatedDate,
                Channel.AvatarPhotoUrl, Channel.Subscribers.Count,
                Channel.Contents.Count, Channel.Owners.Count,
                Channel.TotalLikes, Channel.TotalViews))
            .AsNoTracking().ToArrayAsync();

        logger.LogInformation("Returned {Count} Channels", Channels.Length);

        return Ok(Channels);
    }

    [HttpPut("{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ChannelDto>> UpdateChannel(Guid ChannelId, ChannelUpdateCommand updateDto)
    {
        var Channel = await context.Channels
            .Select(Channel => new
            {
                d = Channel,
                SubscribersCount = Channel.Subscribers.Count,
                ContentsCount = Channel.Contents.Count,
                OwnersCount = Channel.Owners.Count
            })
            .FirstOrDefaultAsync(Channel => Channel.d.Id == ChannelId);

        if (Channel is null || Channel.d is null)
            return NotFound("Channel not found");

        Guid currentUserId = this.GetIDFromClaim();

        ChannelOwner? currentOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.ChannelId == ChannelId);

        if (currentOwner == null)
        {
            logger.LogWarning("User {UserId} tried to update Channel without permission",
                currentUserId);
            return Forbid("You doesn't owner the Channel");
        }

        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("User {UserId} tried to update Channel without permission",
                currentUserId);
            return Forbid("You do not have sufficient rights");
        }

        if (!string.IsNullOrEmpty(updateDto.Description))
            Channel.d.Description = updateDto.Description;

        string slug = updateDto.Name.GenerateSlug();

        if (await context.Channels.AnyAsync(Channel => Channel.Name == updateDto.Name || Channel.Slug == slug))
            return Conflict($"Channel whit name {updateDto.Name} hasted");

        string oldName = Channel.d.Name;

        //Updating
        Channel.d.Name = updateDto.Name;
        Channel.d.Slug = slug;

        ChannelDto responseDto = new(
            Channel.d.Id, Channel.d.Name, "@" + Channel.d.Slug,
            Channel.d.Description ?? "", Channel.d.CreatedDate,
            Channel.d.AvatarPhotoUrl, Channel.SubscribersCount,
            Channel.ContentsCount, Channel.OwnersCount,
            Channel.d.TotalLikes, Channel.d.TotalViews);

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation(
                "Channel {ChannelId} updated successfully. Changed the name from: {OldName} to: {NewName}",
                ChannelId, oldName, updateDto.Name);

            return Ok(responseDto);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while creating Channel for user {UserId}", currentUserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Conflict("Channel name already exists");

            throw;
        }
    }

    [HttpDelete("{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteChannel(Guid ChannelId)
    {
        Guid currentUserId = this.GetIDFromClaim();

        ChannelOwner? currentOwner = await context.ChannelOwners
            .FirstOrDefaultAsync(owner =>
                owner.OwnerId == currentUserId &&
                owner.ChannelId == ChannelId);

        if (currentOwner == null)
        {
            logger.LogWarning("User {UserId} tried to delete Channel {ChannelId} without permission",
                currentUserId, ChannelId);

            return Forbid("You doesn't owner the Channel");
        }

        if (currentOwner.OwnerRole != ChannelOwnerRole.Admin)
        {
            logger.LogWarning("Delete denied for user {UserId} on Channel {ChannelId}",
                currentUserId, ChannelId);

            return Forbid("You do not have sufficient rights");
        }

        Channel? Channel = await context.Channels.FindAsync(ChannelId);

        if (Channel == null)
            return NotFound("Channel not found");

        context.Channels.Remove(Channel);

        await context.SaveChangesAsync();

        logger.LogInformation("Channel {ChannelId} deleted", ChannelId);

        return NoContent();
    }
}