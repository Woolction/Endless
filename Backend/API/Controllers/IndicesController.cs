using Application.Channels.Search.CreateIndex;
using Application.Contents.Search.CreateIndex;
using Application.Users.Search.CreateIndex;
using Application.Searchs.DeleteIndex;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Application.Searchs;
using Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Domain.Common;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IndicesController : ControllerBase
{
    private readonly IMediator mediator;
    public IndicesController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("users")]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult<IndexCreatedDto>> CreateUserIndex(UserCreateIndexCommand cmd)
    {
        Result<IndexCreatedDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                500 => StatusCode(result.StatusCode, result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created("", result.Data);
    }

    [HttpPost("channels")]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult> CreateChannelIndex(ChannelCreateIndexCommand cmd)
    {
        Result<IndexCreatedDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                500 => StatusCode(result.StatusCode, result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created("", result.Data);
    }

    [HttpPost("contents")]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<ActionResult> CreateContentIndex(ContentCreateIndexCommand cmd)
    {
        Result<IndexCreatedDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                500 => StatusCode(result.StatusCode, result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created("", result.Data);
    }

    [HttpDelete]
    [Authorize(Policy = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteIndex(DeleteIndexCommand cmd)
    {
        Result<Null> result = await mediator.Send(cmd);

        if (!result.IsSuccess)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return NoContent();
    } 
}