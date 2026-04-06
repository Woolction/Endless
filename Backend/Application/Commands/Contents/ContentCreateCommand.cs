using Microsoft.AspNetCore.Http;
using Domain.Components;

namespace Application.Commands.Contents;

public record class ContentCreateCommand(
    IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType);