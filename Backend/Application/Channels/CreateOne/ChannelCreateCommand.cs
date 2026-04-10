using Application.Channels.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Channels.CreateOne;

public record class ChannelCreateCommand(
    string Name, IFormFile? AvatarPhoto) : IRequest<Result<ChannelDto>>;