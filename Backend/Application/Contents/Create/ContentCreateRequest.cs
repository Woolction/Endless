using Microsoft.AspNetCore.Http;
using Domain.Common;
using MediatR;
using Application.Contents.Dtos;

namespace Application.Contents.Create;

public record class ContentCreateRequest(
    IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType) : IRequest<Result<ContentDto>>;