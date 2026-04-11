using Application.Channels.Dtos;
using MediatR;

namespace Application.Channels.Choose.One;

public record class ChannelChooseOneQuery(
    Guid Id) : IRequest<Result<ChannelDto>>;