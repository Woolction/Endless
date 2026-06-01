using MediatR;

namespace Application.Features.Channels.Search;

public record class ChannelSearchQuery(
    string Name, double? LastScore) : IRequest<Result<SearchedChannelDto[]>>;