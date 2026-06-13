using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Images.Upload;

public record class ImageUploadMessage(
    Guid Id, ImageOwner Owner, ImageType Type, string Slug, string PhotoPath) : IRequest<Result<Null>>;