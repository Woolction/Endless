using Microsoft.AspNetCore.Http;
using Domain.Common;

namespace Application.Contents.Create;

public record class ContentCreateCommand(
    IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType);