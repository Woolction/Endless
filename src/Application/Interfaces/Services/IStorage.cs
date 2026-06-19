using Microsoft.AspNetCore.Http;
using Application.Features.Images;
using Domain.Common.Enums;

namespace Application.Interfaces.Services;

public interface IStorage
{
    Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos", CancellationToken token = default);
    Task<string> SaveFormFileAsync(IFormFile file, string folderName, CancellationToken token = default);
    string SaveVideo(string folder, string keyPrefix);
    Task<ImageDto> SaveImageVariants(string photoPath, string photoName, (int w, int h)[] sizes, int quality, ImageOwner imageOwner, ImageType imageType = ImageType.Preview, CancellationToken token = default);
}