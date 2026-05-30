using MediatR;

namespace Application.Features.Contents.Video.Upload;

public class VideoUploadHandler : IRequestHandler<VideoUploadMessage, Result<Null>>
{
    public async Task<Result<Null>> Handle(VideoUploadMessage message, CancellationToken token)
    {
        return Result<Null>.Success(200, new Null());
    }
}