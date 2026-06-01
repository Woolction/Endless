using Application.Features.Channels.Dtos;
using MediatR;

namespace Application.Features.Channels.Choose.One;

public record class ChannelChooseOneQuery(
    Guid Id) : IRequest<Result<ChannelDto>>;