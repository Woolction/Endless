using Application.Features.Contents.Dtos;
using Microsoft.AspNetCore.Http;
using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Contents.Create;

public record class ContentCreateRequest(
    IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType) : IRequest<Result<ContentDto>>;