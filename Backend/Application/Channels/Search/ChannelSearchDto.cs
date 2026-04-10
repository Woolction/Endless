using Application.Channels.Dtos;
using Application.Searchs;
using MediatR;

namespace Application.Channels.Search;

public record class ChannelSearchDto(
    ChannelDto[] ChannelsDto, SearchDto? Similariry);