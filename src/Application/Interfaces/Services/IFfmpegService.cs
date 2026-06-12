using Application.Features.Images;

namespace Application.Interfaces.Services;

public interface IFfmpegService
{
    Task<string> UploadGeneratedVideos(string videoPath, string videoName, int height, int fps, CancellationToken token = default);
    Task<string> RunProcess(string args, string fileName = "ffmpeg", CancellationToken token = default);
    Task<double> GetVideoDuration(string videoPath, CancellationToken token = default);
    Task<int> GetVideoHeight(string videoPath, CancellationToken token = default);
    Task<int> GetVideoFps(string videoPath, CancellationToken token = default);
    Task<ImageVariants> GetPhotoFromVideo(string videoPath, string photoName, int height, double timeSeconds = 5, CancellationToken token = default);
}