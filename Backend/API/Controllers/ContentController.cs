using Application.Contents.Create;
using Application.Searchs;
using Application.Contents.Dtos;
using Application.Channels.Dtos;
using Application.Users.Dtos;
using Microsoft.AspNetCore.Authorization;
using Domain.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Infrastructure.Context;
using Domain.Entities;
using API.Extensions;
using Application.Utilities;
using Application.Contents.Recommendate;
using Application.Channels;
using Application;
using Application.Contents.Search;
using MediatR;
using Application.Contents.Create.ForUser;
using Application.Contents.Choose;
using Application.Contents.Random;
using Application.Contents.Update;
using Application.Contents.Delete;
using Application.Users.Choose;
using Application.Channels.Choose.One;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContentController : ControllerBase
{
    private readonly IMediator mediator;

    public ContentController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("Channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentDto>> CreateContent(Guid ChannelId, ContentCreateRequest request)
    {
        ContentCreateCommand cmd = new(
            this.GetIDFromClaim(), ChannelId,
            request.ContentFile,
            request.PrewievPhoto,
            request.Title,
            request.Description,
            request.ContentType
        );

        Result<ContentDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                403 => Forbid(result.Error!),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created($"api/content/{result.Data!.ContentId}", result.Data);
    }

    [HttpPost]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentDto>> CreateContentForUser(ContentCreateRequest request)
    {
        ContentCreateForUserCommand cmd = new(
            this.GetIDFromClaim(),
            request.ContentFile,
            request.PrewievPhoto,
            request.Title,
            request.Description,
            request.ContentType
        );

        Result<ContentDto> result = await mediator.Send(cmd);

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        return Created($"api/content/{result.Data!.ContentId}", result.Data);
    }

    [HttpGet("{ContentId}")]
    public async Task<ActionResult<ChangedContentDto>> GetChangedContent(Guid ContentId)
    {
        ContentChooseQuery contentQuery = new(ContentId);

        Result<ContentDto> resultContent = await mediator.Send(contentQuery);

        if (!resultContent.IsSuccess || resultContent.Data == null)
        {
            return resultContent.StatusCode switch
            {
                404 => NotFound(resultContent.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        UserChooseQuery userQuery = new(resultContent.Data.CreatorId);

        Result<UserDto> resultUser = await mediator.Send(userQuery);

        if (!resultUser.IsSuccess || resultUser.Data == null)
        {
            return resultUser.StatusCode switch
            {
                404 => NotFound(resultUser.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        Result<ChannelDto> resultChannel = new(); 

        if (resultContent.Data.ChannelId != null)
        {
            ChannelChooseOneQuery channelQuery = new(resultContent.Data.ChannelId.Value);

            resultChannel = await mediator.Send(channelQuery);

            if (!resultUser.IsSuccess || resultUser.Data == null)
            {
                return resultUser.StatusCode switch
                {
                    404 => NotFound(resultUser.Error),
                    _ => StatusCode(500, "unknown error")
                };
            }
        }

        return Ok(new ChangedContentDto(
            resultChannel.Data, resultContent.Data, resultUser.Data));
    }

    [HttpGet]
    public async Task<ActionResult<ContentRecoDto[]>> GetRandomContent()
    {
        Result<ContentDto[]> randomContents = await mediator.Send(
            new ContentRandomQuery());

        return Ok(randomContents);
    }

    [HttpGet("Recommendations")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentRecoDto[]>> GetContentForRecommendation()
    {
        Result<ContentRecoDto[]> result = await mediator.Send(new
            ContentRecommendationQuery(this.GetIDFromClaim()));

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

    [HttpGet("Search")]
    public async Task<ActionResult<ContentSearchDto>> GetContentByName([FromQuery] ContentSearchQuery query)
    {
        Result<ContentSearchDto> result = await mediator.Send(query);

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

    [HttpPut("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentDto>> UpdateContent(Guid ContentId, ContentCreateRequest request)
    {
        Guid currentUserId = this.GetIDFromClaim();

        ContentUpdateCommand cmd = new(
            currentUserId, ContentId,
            request.ContentFile,
            request.PrewievPhoto,
            request.Title,
            request.Description,
            request.ContentType);

        Result<ContentDto> result = await mediator.Send(cmd);

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

    [HttpDelete("{ContentId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<IActionResult> DeleteContent(Guid ContentId)
    {
        Result<Null> result = await mediator.Send(new ContentDeleteCommand(
            this.GetIDFromClaim(), ContentId));

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                403 => Forbid(result.Error!),
                _ => StatusCode(500, "unknown error")
            };
        }

        return NoContent();
    }
}