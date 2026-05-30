using Microsoft.AspNetCore.Authorization;
using Application.Features.Contents.Recommendate;
using Application.Features.Channels.Choose.One;
using Application.Features.Contents.Create;
using Application.Features.Contents.Search;
using Application.Features.Contents.Choose;
using Application.Features.Contents.Random;
using Application.Features.Contents.Update;
using Application.Features.Contents.Delete;
using Application.Features.Contents.Dtos;
using Application.Features.Channels.Dtos;
using Microsoft.AspNetCore.Mvc;
using Application.Features.Users.Choose;
using Application.Features.Users.Dtos;
using Application.Utilities;
using Domain.Common.Enums;
using Application;
using MediatR;

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

    [HttpPost("channel/{ChannelId}")]
    [Authorize(Policy = nameof(UserRole.Creator))]
    public async Task<ActionResult<ContentDto>> CreateContent([FromRoute] Guid? ChannelId, [FromForm] ContentCreateRequest request)
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

    [HttpGet("changed/{ContentId}")]
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

    [HttpGet("{ContentId}")]
    public async Task<ActionResult<ContentUrlDto>> GetContentUrlById(Guid ContentId)
    {
        Result<ContentDto> result = await mediator.Send(new ContentChooseQuery(ContentId));

        if (!result.IsSuccess || result.Data == null)
        {
            return result.StatusCode switch
            {
                404 => NotFound(result.Error),
                _ => StatusCode(500, "unknown error")
            };
        }

        if (result.Data.ContentUrl == null)
            return NoContent();

        return Ok(new ContentUrlDto(result.Data.ContentUrl));
    }

    [HttpGet("random")]
    public async Task<ActionResult<ContentFeedDto[]>> GetRandomContent()
    {
        Result<ContentFeedDto[]> randomContents = await mediator.Send(
            new ContentRandomQuery());

        return Ok(randomContents);
    }

    [HttpGet("recommendation")]
    [Authorize(Policy = nameof(UserRole.User))]
    public async Task<ActionResult<ContentFeedDto[]>> GetContentForRecommendation()
    {
        Result<ContentFeedDto[]> result = await mediator.Send(new
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

    [HttpGet("search")]
    public async Task<ActionResult<SearchedContentDto[]>> GetContentByName([FromQuery] ContentSearchQuery query)
    {
        Result<SearchedContentDto[]> result = await mediator.Send(query);

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