using Application.Features.Channels.Dtos;
using MediatR;

namespace Application.Features.Channels.Choose.Many;

public record ChannelChooseManyQuery() : IRequest<Result<ChannelDto[]>>;