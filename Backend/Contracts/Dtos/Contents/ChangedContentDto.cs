using Contracts.Dtos.Channels;
using Contracts.Dtos.Users;

namespace Contracts.Dtos.Contents;

public record class ChangedContentDto(
    ChannelDto? ChannelDto,
    ContentDto ContentDto,
    UserDto? UserDto);