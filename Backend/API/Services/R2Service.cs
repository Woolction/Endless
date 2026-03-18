using Amazon.S3.Transfer;
using Amazon.S3;

namespace Backend.API.Services;

public class R2Service : IR2Service
{
    private readonly IAmazonS3 _s3;

    public R2Service()
    {
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

    public async Task<string> UploadDirectory(string folder, string keyPrefix, string bucketName = "videos")
    {
        var transfer = new TransferUtility(_s3);

        var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string key = $"{keyPrefix}/{Path.GetFileName(file)}";

            await transfer.UploadAsync(file, bucketName, key);
        }

        return $"https://<domain-name>/{keyPrefix}";
    }


    public string SaveVideo(string folder, string keyPrefix)
    {
        string projectRoot = Directory.GetCurrentDirectory();

        string targetDir = Path.Combine(projectRoot, "files", keyPrefix);

        Directory.CreateDirectory(targetDir);

        var files = Directory.GetFiles(folder, "*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            string relative = Path.GetRelativePath(folder, file);
            string destination = Path.Combine(targetDir, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

            File.Copy(file, destination, true);
        }

        return $"/files/{keyPrefix}/master.m3u8";
    }

    public async Task<string> SaveImage(string file, string ext = ".jpeg")
    {
        string id = Guid.NewGuid().ToString();

        string projectRoot = Directory.GetCurrentDirectory();
        string folder = Path.Combine(projectRoot, "files", "images");

        Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, id + ext);

        File.Move(file, path);

        return $"/files/images/{id}{ext}";
    }

    public async Task<string> SaveFormFileAsync(IFormFile file, string folderName, string ext = null!)
    {
        /*if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");*/

        string id = Guid.NewGuid().ToString();
        string projectRoot = Directory.GetCurrentDirectory();
        string folder = Path.Combine(projectRoot, "files", folderName);

        Directory.CreateDirectory(folder);

        string extension = ext ?? Path.GetExtension(file.FileName);
        string filePath = Path.Combine(folder, id + extension);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return filePath;
    }

}