using Application.Dtos.Channels;
using Application.Dtos.Users;

namespace Application.Dtos.Contents;

public record class ChangedContentDto(
    ChannelDto? ChannelDto,
    ContentDto ContentDto,
    UserDto? UserDto);