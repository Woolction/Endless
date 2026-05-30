using Application.Features.Channels.Dtos;
using MediatR;

namespace Application.Features.Channels.Create.Many;

public record class ChannelsCreateCommand(
    Guid UserId, int Count) : IRequest<Result<ChannelDto[]>>;