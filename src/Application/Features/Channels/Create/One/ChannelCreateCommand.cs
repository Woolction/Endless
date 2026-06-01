using Application.Features.Channels.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Channels.Create.One;

public record class ChannelCreateCommand(
    Guid UserId, string Name, IFormFile? IconPhoto) : IRequest<Result<ChannelDto>>;