using Application.Features.Contents.Dtos;
using Microsoft.AspNetCore.Http;
using Domain.Common.Enums;
using MediatR;
using Application.Features.Contents.Update;

namespace Application.Features.Contents.Create;

public record class ContentCreateCommand(
    Guid UserId, Guid? ChannelId, IFormFile? ContentFile, IFormFile? PrewievPhoto,
    string Title, string? Description, ContentType ContentType) : IRequest<Result<ContentUpdateDto>>;