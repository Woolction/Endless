using Application.Channels.Dtos;
using MediatR;

namespace Application.Channels.Create.Many;

public record class ChannelsCreateCommand(
    Guid UserId, int Count) : IRequest<Result<ChannelDto[]>>;