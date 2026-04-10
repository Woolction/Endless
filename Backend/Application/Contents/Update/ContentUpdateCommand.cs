using Application.Contents.Dtos;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Contents.Update;

public record class ContentUpdateCommand(
    Guid UserId, Guid ContentId, IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType) : IRequest<Result<ContentDto>>;