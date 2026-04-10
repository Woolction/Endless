using Application.Channels.Dtos;
using MediatR;

namespace Application.Channels.Create.One;

public class ChannelCreatingHandler : IRequestHandler<ChannelCreateCommand, Result<ChannelDto>>
{
    public Task<Result<ChannelDto>> Handle(ChannelCreateCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}