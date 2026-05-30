using Microsoft.AspNetCore.Http;
using Application.Features.Rows.Contents;
using Domain.Common.Enums;

namespace Application.Interfaces.Services;

public interface IR2Service
{
    Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos", CancellationToken token = default);
    Task<string> SaveFormFileAsync(IFormFile file, string folderName, CancellationToken token = default);
    string SaveVideo(string folder, string keyPrefix);
    Task<PhotoVariants> SavePhotoVariants(string photoPath, string photoName, CancellationToken token = default);
    Task<PhotoVariants> SaveIconVariants(string photoPath, string photoName, IconType type, CancellationToken token = default);
}