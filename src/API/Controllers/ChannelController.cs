using Application.Features.Channels.Choose.Many;
using Application.Features.Channels.Create.Many;
using Application.Features.Channels.Create.One;
using Application.Features.Channels.Choose.One;
using Application.Features.Channels.Delete;
using Application.Features.Channels.Update;
using Application.Features.Channels.Search;
using Microsoft.AspNetCore.Authorization;
using Application.Features.Channels.Dtos;
using Microsoft.AspNetCore.Mvc;
using Application.Utilities;
using Domain.Common.Enums;
using Application;
using MediatR;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChannelController : ControllerBase
{
    private readonly IMediator mediator;

    public ChannelController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ChannelUpdateDto>> CreateChannel([FromForm] ChannelCreateRequest request)
    {
        ChannelCreateCommand cmd = new(
            this.GetIDFromClaim(), request.Name, request.IconPhoto);

        Result<ChannelUpdateDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                409 => Conflict(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created($"api/Channel/{result.Data.Id}", result.Data);
    }

    [HttpPost("many")]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult<ChannelUpdateDto[]>> CreateChannels(ChannelsCreateRequest request)
    {
        if (request.Count < 1)
            return BadRequest($"Count < 1: {request.Count}");

        ChannelsCreateCommand cmd = new(
            this.GetIDFromClaim(), request.Count);

        Result<ChannelUpdateDto[]> result = await mediator.Send(cmd);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                409 => Conflict(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created("", result.Data);
    }

    [HttpGet("{ChannelId}")]
    public async Task<ActionResult<ChannelDto>> GetChannelById(Guid ChannelId)
    {
        ChannelChooseOneQuery query = new(ChannelId);

        Result<ChannelDto> result = await mediator.Send(query);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Ok(result.Data);
    }

    [HttpGet("search")]
    public async Task<ActionResult<SearchedChannelDto[]>> GetChannelsByName([FromQuery] ChannelSearchQuery query)
    {
        Result<SearchedChannelDto[]> result = await mediator.Send(query);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "Unknown error")
            };
        }

        return Ok(result.Data);
    }

    [HttpGet]
    public async Task<ActionResult<ChannelDto[]>> GetChannels()
    {
        Result<ChannelDto[]> result = await mediator.Send(
            new ChannelChooseManyQuery());

        if (result.Data == null)
        {
            return StatusCode(500, "unknown error");
        }

        return Ok(result.Data);
    }

    [HttpPut("{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ChannelUpdateDto>> UpdateChannel([FromRoute] Guid ChannelId, [FromForm] ChannelUpdateRequest request)
    {
        ChannelUpdateCommand cmd = new(
            this.GetIDFromClaim(), ChannelId, request.Name, request.Description, request.IconPhoto);

        Result<ChannelUpdateDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                403 => Forbid(result.Error!),
                409 => Conflict(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Ok(result.Data);
    }

    [HttpDelete("{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteChannel(Guid ChannelId)
    {
        ChannelDeleteCommand cmd = new(
            this.GetIDFromClaim(), ChannelId);

        Result<Null> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                403 => Forbid(result.Error!),
                404 => NotFound(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return NoContent();
    }
}