using Domain.Rows.Contents;
using Microsoft.AspNetCore.Http;

namespace Domain.Common.Interfaces.Services;

public interface IR2Service
{
    Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos", CancellationToken token = default);
    Task<string> SaveFormFileAsync(IFormFile file, string folderName, CancellationToken token = default);
    string SaveVideo(string folder, string keyPrefix);
    Task<PreviewPhotoVariants> SavePhotoVariants(string file, string fileName);
}