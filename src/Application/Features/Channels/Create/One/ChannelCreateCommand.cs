using Application.Features.Channels.Update;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace Application.Features.Channels.Create.One;

public record class ChannelCreateCommand(
    Guid UserId, string Name, IFormFile? IconPhoto) : IRequest<Result<ChannelUpdateDto>>;