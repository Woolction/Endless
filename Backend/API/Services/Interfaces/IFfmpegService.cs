namespace Backend.API.Services.Interfaces;

public interface IFfmpegService
{
    Task<string> UploadGeneratedVideos(string inputFile);
    Task<int> GetVideoDuration(string filePath);
}