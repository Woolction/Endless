using Application.Features.Rows.Contents;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Domain.Common.Enums;
using Amazon.S3.Transfer;
using Amazon.S3;

namespace Storage.Services;

public class R2Storage : IStorage
{
    private readonly IImageAnalyzer imageAnalyzer;
    //private readonly IAmazonS3 _s3;

    public R2Storage(IImageAnalyzer imageAnalyzer)
    {
        this.imageAnalyzer = imageAnalyzer;

        /*var config = new AmazonS3Config
        {
            ServiceURL = "https://<account_id>.r2.cloudflarestorage.com",
            ForcePathStyle = true
        };

        _s3 = new AmazonS3Client(
                "<access_key>",
                "<secret_key>",
            config

        );*/
    }

    public async Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos", CancellationToken token = default)
    {
        /*var transfer = new TransferUtility(_s3);

        var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string key = $"{keyPrefix}/{Path.GetFileName(file)}";

            await transfer.UploadAsync(file, bucketName, key, token);
        }*/

        return $"https://<Channel-name>/{keyPrefix}";
    }

    public async Task<string> SaveFormFileAsync(IFormFile file, string folderName, CancellationToken token = default)
    {
        string id = Guid.NewGuid().ToString();
        string projectRoot = Directory.GetCurrentDirectory();
        string folder = Path.Combine(projectRoot, "files", folderName);

        Directory.CreateDirectory(folder);

        string extension = Path.GetExtension(file.FileName);
        string filePath = Path.Combine(folder, id + extension);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, token);

        return filePath;
    }

    public string SaveVideo(string folder, string keyPrefix)
    {
        string targetDir = Path.Combine("/storage", keyPrefix);

        Directory.CreateDirectory(targetDir);

        var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string relative = Path.GetRelativePath(folder, file);
            string destination = Path.Combine(targetDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

            File.Copy(file, destination, true);
        }

        return $"/storage/{keyPrefix}/master.m3u8";
    }

    public async Task<PhotoVariants> SavePhotoVariants(string photoPath, string photoName, CancellationToken token = default)
    {
        string folder = Path.Combine("/storage/images/content-previews", photoName);
        Directory.CreateDirectory(folder);

        await imageAnalyzer.GenerateImageVariants(
            photoPath, folder, [(1280, 720), (960, 540), (640, 360)], 80, token);

        return new PhotoVariants(
            folder,
            "640x360.webp",
            "960x540.webp",
            "1280x720.webp"
        );
    }

    public async Task<PhotoVariants> SaveIconVariants(string photoPath, string photoName, IconType type, CancellationToken token = default)
    {
        string folder = Path.Combine($"/storage/images/{type.ToString().ToLower()}-icons", photoName);
        Directory.CreateDirectory(folder);

        await imageAnalyzer.GenerateImageVariants(
            photoPath, folder, [(256, 256), (128, 128), (64, 64)], 85, token);

        return new PhotoVariants(
            folder, "64x64.webp", "128x128.webp", "256x256.webp"
        );
    }
}