using Application.Channels.Dtos;
using MediatR;

namespace Application.Channels.CreateMany;

public record class ChannelsCreateCommand(int Count) : IRequest<Result<ChannelDto[]>>
{
    public Guid? UserId;
}