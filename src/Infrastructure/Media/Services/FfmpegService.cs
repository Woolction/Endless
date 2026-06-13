using Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using System.Globalization;
using System.Diagnostics;
using System.Text;
using Domain.Common.Enums;

namespace Media.Services;

public class FfmpegService : IFfmpegService
{
    private readonly ILogger<FfmpegService> logger;
    private readonly IStorage Storage;

    public FfmpegService(ILogger<FfmpegService> logger, IStorage Storage)
    {
        this.Storage = Storage;
        this.logger = logger;
    }

    public async Task<string> UploadGeneratedVideos(string videoPath, string videoName, int height, int fps, CancellationToken token = default)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            Directory.CreateDirectory(tempDir);

            await GenerateHlsVariants(videoPath, tempDir, height, fps, token);

            string folderKey = $"videos/{videoName}";

            string url = Storage.SaveVideo(tempDir, folderKey);

            return url;
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private async Task GenerateHlsVariants(string videoPath, string outputDir, int height, int fps, CancellationToken token = default)
    {
        fps = Math.Clamp(fps, 12, 60);

        var variants = new List<int>();

        if (height >= 1080)
            variants.AddRange([360, 480, 720, 1080]);
        else if (height >= 720)
            variants.AddRange([360, 480, 720]);
        else if (height >= 480)
            variants.AddRange([360, 480]);
        else if (height >= 360)
            variants.AddRange([360]);

        var split = new StringBuilder($"[0:v]split={variants.Count}");
        var filters = new StringBuilder();
        var maps = new StringBuilder();

        var streamMaps = new List<string>();

        int gap = fps * 2;

        for (int i = 0; i < variants.Count; i++)
        {
            int variant = variants[i];

            filters.Append($"[v{i}]fps={fps},scale=-2:{variant}[v{variant}];");
            split.Append($"[v{i}]");

            maps.Append($"-map \"[v{variant}]\" ");
            maps.Append("-map 0:a:0 ");

            maps.Append($"-c:v:{i} libx264 -preset veryfast ");
            maps.Append($"-b:v:{i} {GetBitrate(variant, fps)} ");

            maps.Append($"-c:a:{i} aac -b:a:{i} 128k ");

            maps.Append($"-g {gap} -keyint_min {gap} -sc_threshold 0 ");

            streamMaps.Add($"v:{i},a:{i}");
        }

        split.Append(';');

        string streamMap = string.Join(" ", streamMaps);

        string args =
            $"-i \"{videoPath}\" " +
            $"-filter_complex \"{split.ToString()}{filters.ToString()}\" " +
            maps.ToString() +
            "-f hls " +
            "-hls_time 4 " +
            "-hls_playlist_type vod " +
            "-hls_segment_filename \"" + Path.Combine(outputDir, "stream_%v_%03d.ts") + "\" " +
            "-master_pl_name master.m3u8 " +
            $"-var_stream_map \"{streamMap}\" " +
            $"{Path.Combine(outputDir, "stream_%v.m3u8")}";

        await RunProcess(args, token: token);
    }

    public async Task<int> GetVideoHeight(string videoPath, CancellationToken token = default)
    {
        string output = await RunProcess(
            $"-v error -select_streams v:0 -show_entries stream=height -of csv=p=0 \"{videoPath}\"",
            "ffprobe", token);

        return int.Parse(output.Trim());
    }

    public async Task<int> GetVideoFps(string videoPath, CancellationToken token = default)
    {
        string output = await RunProcess(
            $"-v error -select_streams v:0 " +
            $"-show_entries stream=r_frame_rate " +
            $"-of default=noprint_wrappers=1:nokey=1 " +
            $"\"{videoPath}\"",
            "ffprobe",
            token);

        string value = output.Trim();

        if (value.Contains('/'))
        {
            string[] parts = value.Split('/');

            double numerator = double.Parse(parts[0]);
            double denominator = double.Parse(parts[1]);

            return (int)Math.Round(numerator / denominator);
        }

        return (int)Math.Round(double.Parse(value));
    }

    public async Task<string> RunProcess(string args, string fileName = "ffmpeg", CancellationToken token = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = args,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new Exception("failed to start ffmpeg");

        var errorTask = process.StandardError.ReadToEndAsync(token);
        var outputTask = process.StandardOutput.ReadToEndAsync(token);

        await process.WaitForExitAsync(token);

        string error = await errorTask;
        string output = await outputTask;

        if (process.ExitCode != 0)
            throw new Exception($"{fileName} failed with code: {process.ExitCode}\n{error}");

        logger.LogInformation("{fileName} finished process: {output}", fileName, output);

        return output;
    }

    public async Task<double> GetVideoDuration(string videoPath, CancellationToken token = default)
    {
        string output = await RunProcess(
            $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{videoPath}\"",
            "ffprobe", token);

        double seconds = double.Parse(output, CultureInfo.InvariantCulture);

        return Math.Round(seconds);
    }

    public async Task<ImageVariantsDto> GetPhotoFromVideo(string videoPath, int height, double timeSeconds, string imageName, (int w, int h)[] sizes, int quality, ImageOwner imageOwner, ImageType imageType, CancellationToken token = default)
    {
        string folder = Path.Combine($"/storage/images/{imageOwner.ToString().ToLower()}-{imageType.ToString().ToLower()}", imageName);
        Directory.CreateDirectory(folder);

        var variants = new List<ImageVariantDto>();

        var useableSizes = new List<(int w, int h)>();

        for (int i = 0; i < sizes.Length; i++)
        {
            var (w, h) = sizes[i];

            if (height >= h)
            {
                useableSizes.Add((w, h));
            }
        }

        for (int i = 0; i < sizes.Length; i++)
        {
            var (w, h) = sizes[i];

            string output = Path.Combine(folder, $"{w}x{h}.webp");

            await RunProcess(
                $"-ss {timeSeconds} -i \"{videoPath}\" " +
                $"-vf \"scale={w}:{h}:force_original_aspect_ratio=increase:flags=lanczos,crop={w}:{h}\" " +
                $"-frames:v 1 -c:v libwebp -quality {quality} \"{output}\"", token: token);

            variants.Add(
                new ImageVariantDto() { Url = output, Width = w, Height = h });
        }

        return new ImageVariantsDto(
            folder, variants);
    }

    private string GetBitrate(int height, int fps)
    {
        double baseRate = height switch
        {
            360 => 800,
            480 => 1400,
            720 => 2800,
            1080 => 6000,
            _ => 800
        };

        double factor = fps switch
        {
            <= 30 => 1.0,
            <= 60 => 1.5,
            _ => 1.5
        };

        return $"{(int)(baseRate * factor)}k";
    }
}