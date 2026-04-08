using Application.Dtos.Searchs;

namespace Application.Dtos.Channels;

public record class ChannelSearchDto(
    ChannelDto[] ChannelsDto, SearchDto? Similariry);