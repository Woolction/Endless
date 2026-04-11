using Microsoft.AspNetCore.Http;

namespace Application.Channels.Create.One;

public record class ChannelCreateRequest(string Name, IFormFile? AvatarPhoto);