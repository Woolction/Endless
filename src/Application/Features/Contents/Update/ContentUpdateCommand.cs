using Microsoft.AspNetCore.Http;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Contents.Update;

public record class ContentUpdateCommand(
    Guid UserId, Guid ContentId, IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType) : IRequest<Result<ContentUpdateDto>>;