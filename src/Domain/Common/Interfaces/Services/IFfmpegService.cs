namespace Domain.Common.Interfaces.Services;

public interface IFfmpegService
{
    Task<string> UploadGeneratedVideos(string videoPath, string videoName, CancellationToken token = default);
    Task<string> RunProcess(string args, string fileName = "ffmpeg", CancellationToken token = default);
    Task<double> GetVideoDuration(string videoPath, CancellationToken token = default);
    Task<string> GetPhotoFromVideo(string videoPath, string photoName, string outputPath = "/storage/images/previewPhoto.jpeg", double timeSeconds = 5, CancellationToken token = default);
}