namespace Backend.API.Services;

public interface IFfmpegService
{
    Task<string> UploadGeneratedVideos(string inputFile);
    int GetVideoDuration(string filePath);
}