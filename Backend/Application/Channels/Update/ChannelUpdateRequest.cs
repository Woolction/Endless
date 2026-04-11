using Application.Channels.Dtos;
using MediatR;

namespace Application.Channels.Update;

public record class ChannelUpdateRequest(
    string Name, string Description) : IRequest<Result<ChannelDto>>;