using Application.Features.Channels.Dtos;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace Application.Features.Channels.Update;

public record class ChannelUpdateCommand(
    Guid UserId, Guid ChannelId, string Name, string Description, IFormFile? IconPhoto) : IRequest<Result<ChannelUpdateDto>>;