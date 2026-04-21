using Application.Searchs;
using MediatR;

namespace Application.Channels.Search;

public record class ChannelSearchQuery(
    string Name, double? LastScore) : IRequest<Result<SearchedChannelDto[]>>;