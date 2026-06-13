using Application.Features.Channels.Update;
using MediatR;

namespace Application.Features.Channels.Create.Many;

public record class ChannelsCreateCommand(
    Guid UserId, int Count) : IRequest<Result<ChannelUpdateDto[]>>;