using Contracts.Dtos.Searchs;

namespace Contracts.Dtos.Channels;

public record class ChannelsearchQuery(
    ChannelDto[] ChannelsDto, SearchDto? Similariry);