using Microsoft.AspNetCore.Http;
using Application.Features.Images;
using Domain.Common.Enums;

namespace Application.Interfaces.Services;

public interface IStorage
{
    Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos", CancellationToken token = default);
    Task<string> SaveFormFileAsync(IFormFile file, string folderName, CancellationToken token = default);
    string SaveVideo(string folder, string keyPrefix);
    Task<ImageVariantsDto> SaveImageVariantsDto(string photoPath, string photoName, CancellationToken token = default);
    Task<ImageVariantsDto> SaveIconVariants(string photoPath, string photoName, IconType type, CancellationToken token = default);
}