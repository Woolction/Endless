namespace Backend.API.Services;

public interface IR2Service
{
    Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos");
    string SaveVideo(string folder, string keyPrefix);
    Task<string> SaveImage(string file, string ext = ".jpeg");
}