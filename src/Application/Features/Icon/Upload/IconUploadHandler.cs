using MediatR;

namespace Application.Features.Icon.Upload;

public class IconUploadHandler : IRequestHandler<IconUploadMessage, Result<Null>>
{
    public IconUploadHandler()
    {
        
    }
    
    public async Task<Result<Null>> Handle(IconUploadMessage message, CancellationToken token)
    {
        return Result<Null>.Success(200, new Null());
    }
}