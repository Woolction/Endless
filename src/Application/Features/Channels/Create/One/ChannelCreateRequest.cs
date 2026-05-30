using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Channels.Create.One;

public record class ChannelCreateRequest(
    string Name, IFormFile? IconPhoto);