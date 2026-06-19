using Application.Features.Images;
using Domain.Common.Enums;

namespace Application.Interfaces.Services;

public interface IFfmpegService
{
    Task<string> UploadGeneratedVideos(string videoPath, string videoName, int height, int fps, CancellationToken token = default);
    Task<string> RunProcess(string args, string fileName = "ffmpeg", CancellationToken token = default);
    Task<double> GetVideoDuration(string videoPath, CancellationToken token = default);
    Task<int> GetVideoHeight(string videoPath, CancellationToken token = default);
    Task<int> GetVideoFps(string videoPath, CancellationToken token = default);
    Task<ImageDto> GetPhotoFromVideo(string videoPath, int height, double timeSeconds, string imageName, (int w, int h)[] sizes, int quality, ImageOwner imageOwner, ImageType imageType, CancellationToken token = default);
}