namespace Backend.API.Services.Interfaces;

public interface IR2Service
{
    Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos");
    string SaveVideo(string folder, string keyPrefix);
    Task<string> SaveImage(string file, string ext = ".jpeg");
    Task<string> SaveFormFileAsync(IFormFile file, string folderName, string ext = null!);
}