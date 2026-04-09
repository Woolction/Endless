using Application.Channels.Dtos;
using Application.Users.Dtos;

namespace Application.Contents.Dtos;

public record class ChangedContentDto(
    ChannelDto? ChannelDto,
    ContentDto ContentDto,
    UserDto? UserDto);