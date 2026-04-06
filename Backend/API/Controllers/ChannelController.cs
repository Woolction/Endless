using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Application.Commands.Channels;
using Application.Queries.Searchs;
using Domain.Interfaces.Services;
using Contracts.Dtos.Channels;
using Microsoft.AspNetCore.Mvc;
using Contracts.Dtos.Searchs;
using Infrastructure.Context;
using Application.Utilities;
using Domain.Components;
using Domain.Entities;
using Npgsql;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelController : ControllerBase
{
    private readonly EndlessContext context;

    private readonly ILogger<ChannelController> logger;
    private readonly IR2Service r2Service;

    public ChannelController(EndlessContext context, IR2Service r2Service, ILogger<ChannelController> logger)
    {
        this.context = context;

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

        Channel Channel = new()
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

            Channel.AvatarPhotoUrl = photoUrl;

            System.IO.File.Delete(photoPath);
        }

        ChannelOwner ChannelOwner = new()
        {
            OwnerId = currentUserId,
            Channel = Channel,
            OwnedDate = DateTime.UtcNow,
            OwnerRole = ChannelOwnerRole.Admin
        };

        ChannelSubscription ChannelSubscription = new()
        {
            Channel = Channel,
            SubscriberId = currentUserId,
            SubscribedDate = DateTime.UtcNow,
            Notification = false
        };

        context.ChannelSubscriptions.Add(ChannelSubscription);
        context.ChannelOwners.Add(ChannelOwner);
        context.Channels.Add(Channel);

        try
        {
            await context.SaveChangesAsync();

            logger.LogInformation("Channel {ChannelId} created with slug {Slug}",
                Channel.Id, slug);

            return Created($"api/Channel/{Channel.Id}", new ChannelDto(
                Channel.Id,
                Channel.Name,
                "@" + Channel.Slug,
                Channel.Description ?? "",
                Channel.CreatedDate,
                Channel.AvatarPhotoUrl,
                1, 0, 1,
                Channel.TotalLikes,
                Channel.TotalViews));
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Error while creating Channel for user {UserId}", currentUserId);

            if (ex.InnerException is PostgresException pg && pg.SqlState == "23505")
                return Conflict("Channel name already exists");

            throw;
        }
    }

    [HttpGet("{ChannelId}")]
    public async Task<ActionResult<ChannelDto>> GetChannelById(Guid ChannelId)
    {
        ChannelDto? Channel = await context.Channels
            .Select(Channel => new ChannelDto(
                Channel.Id, Channel.Name, "@" + Channel.Slug,
                Channel.Description ?? "", Channel.CreatedDate,
                Channel.AvatarPhotoUrl, Channel.Subscribers.Count,
                Channel.Contents.Count, Channel.Owners.Count,
                Channel.TotalLikes, Channel.TotalViews))
            .AsNoTracking()
            .FirstOrDefaultAsync(Channel => Channel.Id == ChannelId);

        if (Channel == null)
            return NotFound("Channel not found");

        logger.LogInformation("Getted Channel {ChannelId}",
            ChannelId);

        return Ok(Channel);
    }

    [HttpGet("search")]
    public async Task<ActionResult<ChannelsearchQuery>> GetChannelsForName([FromQuery] SearchQuery requestDto)
    {
        IQueryable<Channel> query = context.Channels.AsQueryable();

        if (requestDto.LastSearch is not null)
        {
            query = query.Where(Channel =>
                EF.Functions.ILike(Channel.Name, $"%{requestDto.Name}%") == requestDto.LastSearch.LastLiked &&
                EF.Functions.TrigramsSimilarity(Channel.Name, requestDto.Name) < requestDto.LastSearch.LastSimilarity &&
                EF.Functions.FuzzyStringMatchLevenshtein(Channel.Name, requestDto.Name) >= requestDto.LastSearch.LastLevenshit);
        }
        else
        {
            query = query.Where(Channel =>
                EF.Functions.ILike(Channel.Name, $"%{requestDto.Name}%") ||
                EF.Functions.TrigramsSimilarity(Channel.Name, requestDto.Name) > 0.2f ||
                EF.Functions.FuzzyStringMatchLevenshtein(Channel.Name, requestDto.Name) <= 3);
        }

        var Channels = await query
            .OrderByDescending(Channel => EF.Functions.TrigramsSimilarity(Channel.Name, requestDto.Name))
            .Take(20)
            .Select(Channel => new
            {
                Channel = new ChannelDto(
                    Channel.Id, Channel.Name, "@" + Channel.Slug,
                    Channel.Description ?? "", Channel.CreatedDate,
                    Channel.AvatarPhotoUrl, Channel.Subscribers.Count,
                    Channel.Contents.Count, Channel.Owners.Count,
                    Channel.TotalLikes, Channel.TotalViews),
                LastLiked = EF.Functions.ILike(Channel.Name, $"%{requestDto.Name}%"),
                LastSimilarity = EF.Functions.TrigramsSimilarity(Channel.Name, requestDto.Name),
                LastLevenshit = EF.Functions.FuzzyStringMatchLevenshtein(Channel.Name, requestDto.Name)
            })
            .AsNoTracking().ToArrayAsync();

        var lastResponse = Channels.LastOrDefault();

        ChannelDto[] ChannelResponses = Channels.Select(Channel => Channel.Channel).ToArray();

        logger.LogInformation("Search returned Channels {Count} results for {Query}",
            Channels.Length, requestDto.Name);

        return Ok(new ChannelsearchQuery(
            ChannelResponses, lastResponse == null ? null : GetSearchQuery(
                lastResponse.LastLiked, lastResponse.LastSimilarity, lastResponse.LastLevenshit)));
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
            .Where(Channel => Channel.Id == ChannelId)
            .Select(Channel => new
            {
                d = Channel,
                SubscribersCount = Channel.Subscribers.Count,
                ContentsCount = Channel.Contents.Count,
                OwnersCount = Channel.Owners.Count
            })// For mapping
            .FirstOrDefaultAsync();

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

    private SearchDto GetSearchQuery(bool IsLastLiked, double LastSimilarity, int LastLevenshit)
    {
        return new()
        {
            LastLiked = IsLastLiked,
            LastSimilarity = LastSimilarity,
            LastLevenshit = LastLevenshit
        };
    }
}