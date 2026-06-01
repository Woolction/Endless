using Application.Features.Channels.Dtos;
using Application.Features.Users.Dtos;

namespace Application.Features.Contents.Dtos;

public record class ChangedContentDto(
    ChannelDto? ChannelDto,
    ContentDto ContentDto,
    UserDto? UserDto);