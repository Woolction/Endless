using Application.Contents.Dtos;
using Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Contents.CreateForUser;

public record class ContentCreateForUserCommand(
    Guid UserId, IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType) : IRequest<Result<ContentDto>>;