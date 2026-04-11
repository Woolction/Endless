using MediatR;

namespace Application.Channels.Delete;

public record class ChannelDeleteCommand(
    Guid UserId, Guid ChannelId) : IRequest<Result<Null>>;