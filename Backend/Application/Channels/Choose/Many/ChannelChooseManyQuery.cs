using Application.Channels.Dtos;
using MediatR;

namespace Application.Channels.Choose.Many;

public record ChannelChooseManyQuery() : IRequest<Result<ChannelDto[]>>;