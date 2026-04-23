using Application.Channels.Dtos;
using Application.Searchs;
using MediatR;

namespace Application.Channels.Search;

public record class SearchedChannelDto(
    ChannelDto ChannelsDto, double Score);