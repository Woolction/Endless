using Domain.Common.Enums;
using MediatR;

namespace Application.Features.Contents.Video.Upload;

public record class VideoUploadMessage(
    Guid ContentId, string Slug, string? VideoPath, string? ImagePath, ImageOwner ImageOwner, ImageType ImageType
    ) : IRequest<Result<Null>>;