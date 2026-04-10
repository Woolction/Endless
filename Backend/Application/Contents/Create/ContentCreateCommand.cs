using Microsoft.AspNetCore.Http;
using Domain.Common;
using MediatR;

namespace Application.Contents.Create;

public record class ContentCreateCommand(
    IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType) : IRequest
{
    public Guid CurrentUserId { get; set; }
    public Guid ChannelId { get; set; }
}