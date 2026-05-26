using Application.Channels.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Channels.Create.One;

public record class ChannelCreateCommand(
    Guid UserId, string Name, IFormFile? IconPhoto) : IRequest<Result<ChannelDto>>;