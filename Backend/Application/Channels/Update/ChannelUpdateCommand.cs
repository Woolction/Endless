using Application.Channels.Dtos;
using MediatR;

namespace Application.Channels.Update;

public record class ChannelUpdateCommand(
    Guid UserId, Guid ChannelId, string Name, string Description) : IRequest<Result<ChannelDto>>;