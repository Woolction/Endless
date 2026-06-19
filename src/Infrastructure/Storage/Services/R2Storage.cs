using Application.Interfaces.Services;
using Application.Features.Images;
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

    public Task<ImageDto> SaveImageVariants(string photoPath, string photoName, (int w, int h)[] sizes, int quality, ImageOwner imageOwner, ImageType imageType = ImageType.Preview, CancellationToken token = default)
    {
        string folder = Path.Combine($"/storage/images/{imageOwner.ToString().ToLower()}-{imageType.ToString().ToLower()}", photoName);
        Directory.CreateDirectory(folder);

        return imageAnalyzer.GenerateImageVariantsDto(
            photoPath, folder, sizes, quality, token);

        /*new ImageVariantsDto(
            folder, "640x360.webp", "960x540.webp", "1280x720.webp"
        );*/
    }
}