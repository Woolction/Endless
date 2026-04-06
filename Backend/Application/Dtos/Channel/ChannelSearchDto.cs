using Application.Dtos.Searchs;

namespace Application.Dtos.Channels;

public record class ChannelsearchQuery(
    ChannelDto[] ChannelsDto, SearchDto? Similariry);