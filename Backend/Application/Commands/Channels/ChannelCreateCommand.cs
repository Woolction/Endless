using Microsoft.AspNetCore.Http;

namespace Application.Commands.Channels;

public record class ChannelCreateCommand(
    string Name, IFormFile? AvatarPhoto);