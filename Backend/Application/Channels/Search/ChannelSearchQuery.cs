using Application.Searchs;
using MediatR;

namespace Application.Channels.Search;

public record class ChannelSearchQuery(
    string Name, SearchDto? LastSearch) : IRequest<Result<ChannelSearchDto>>;