using Application.Channels.Dtos;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace Application.Channels.Update;

public record class ChannelUpdateCommand(
    Guid UserId, Guid ChannelId, string Name, string Description, IFormFile? IconPhoto) : IRequest<Result<ChannelDto>>;