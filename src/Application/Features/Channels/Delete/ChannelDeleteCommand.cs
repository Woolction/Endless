using MediatR;

namespace Application.Features.Channels.Delete;

public record class ChannelDeleteCommand(
    Guid UserId, Guid ChannelId) : IRequest<Result<Null>>;