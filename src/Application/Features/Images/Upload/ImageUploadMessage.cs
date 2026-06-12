using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Images.Upload;

public record class ImageUploadMessage(
    Guid Id, IconType Type, string Slug, string PhotoPath) : IRequest<Result<Null>>;