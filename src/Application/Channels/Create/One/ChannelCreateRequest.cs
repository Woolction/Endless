using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Channels.Create.One;

public record class ChannelCreateRequest(
    string Name, IFormFile? IconPhoto);