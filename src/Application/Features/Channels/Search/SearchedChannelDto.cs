using Application.Features.Channels.Dtos;

namespace Application.Features.Channels.Search;

public record class SearchedChannelDto(
    ChannelDto ChannelsDto, double Score);