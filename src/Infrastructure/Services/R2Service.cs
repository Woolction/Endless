using Application.Interfaces.Services;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using Application.Features.Rows.Contents;
using Domain.Common.Enums;
using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.AspNetCore.WebUtilities;
using SixLabors.ImageSharp.Formats.Ico;

namespace Infrastructure.Services;

public class R2Service : IR2Service
{
    /*private readonly IAmazonS3 _s3;

    public R2Service()
    {
        var config = new AmazonS3Config
        {
            ServiceURL = "https://<account_id>.r2.cloudflarestorage.com",
            ForcePathStyle = true
        };

        _s3 = new AmazonS3Client(
                "<access_key>",
                "<secret_key>",
            config

        );
    }*/

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
        /*if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");*/

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

        using var image = await Image.LoadAsync(photoPath, token);

        var sizes = new List<(int w, int h)>();

        if (image.Width >= 1280 && image.Height >= 720)
        {
            sizes.Add((1280, 720));
            sizes.Add((960, 540));
            sizes.Add((640, 360));
        }
        else if (image.Width >= 960 && image.Height >= 540)
        {
            sizes.Add((960, 540));
            sizes.Add((640, 360));
        }
        else if (image.Width >= 640 && image.Height >= 360)
        {
            sizes.Add((640, 360));
        }

        for (int i = 0; i < sizes.Count; i++)
        {
            var (w, h) = sizes[i];

            using var clone = image.Clone(x => x
                .Resize(new ResizeOptions()
                {
                    Size = new Size(w, h),
                    Mode = ResizeMode.Crop,
                    Sampler = KnownResamplers.Lanczos3
                }));

            string output = Path.Combine(folder, $"{w}x{h}.webp");

            await clone.SaveAsWebpAsync(output, new WebpEncoder()
            {
                Quality = 80
            }, cancellationToken: token);
        }

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

        using var image = await Image.LoadAsync(photoPath, token);

        var sizes = new List<(int w, int h)>();

        if (image.Width >= 256 && image.Height >= 256)
        {
            sizes.Add((256, 256));
            sizes.Add((128, 128));
            sizes.Add((64, 64));
        }
        else if (image.Width >= 128 && image.Height >= 128)
        {
            sizes.Add((128, 128));
            sizes.Add((64, 64));
        }
        else if (image.Width >= 64 && image.Height >= 64)
        {
            sizes.Add((64, 64));
        }

        for (int i = 0; i < sizes.Count; i++)
        {
            var (w, h) = sizes[i];

            using var clone = image.Clone(x => x
                .Resize(new ResizeOptions()
                {
                    Size = new Size(w, h),
                    Mode = ResizeMode.Crop,
                    Sampler = KnownResamplers.Lanczos3
                }));

            string output = Path.Combine(folder, $"{w}x{h}.webp");

            await clone.SaveAsWebpAsync(output, new WebpEncoder()
            {
                Quality = 85
            }, token);
        }

        return new PhotoVariants(
            folder, "64x64.webp", "128x128.webp", "256x256.webp"
        );
    }
}