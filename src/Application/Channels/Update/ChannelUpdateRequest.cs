using Application.Channels.Dtos;
using Microsoft.AspNetCore.Http;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Application.Channels.Update;

public record class ChannelUpdateRequest(
    string Name, string Description, IFormFile? IconPhoto) : IRequest<Result<ChannelDto>>;