using Microsoft.AspNetCore.Http;

namespace Domain.Common.Interfaces.Services;

public interface IR2Service
{
    Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos", CancellationToken token = default);
    Task<string> SaveFormFileAsync(IFormFile file, string folderName, string ext = null!, CancellationToken token = default);
    string SaveVideo(string folder, string keyPrefix);
    string SaveImage(string file, string fileName, string ext = ".jpeg");
}