using MediatR;

namespace Application.Features.Contents.Video.Upload;

public record class VideoUploadMessage(
    Guid ContentId, string Slug, string? VideoPath, string? PhotoPath
    ) : IRequest<Result<Null>>;