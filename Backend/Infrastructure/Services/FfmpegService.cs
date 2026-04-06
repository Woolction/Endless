using Domain.Interfaces.Services;
using System.Globalization;
using System.Diagnostics;

namespace Infrastructure.Services;

public class FfmpegService : IFfmpegService
{
    private const string FfmpegPath = @"C:\Program Files\ffmpeg-essentials\bin\ffmpeg.exe";

    public readonly IR2Service r2Service;

    public FfmpegService(IR2Service r2Service)
    {
        this.r2Service = r2Service;
    }

    /*public async Task<string> UploadGeneratedVideos(string inputFile)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        Directory.CreateDirectory(tempDir);

        try
        {
            var variants = new[]
            {
                new { Height = 360, Bitrate = 800000 },
                new { Height = 480, Bitrate = 1400000 },
                new { Height = 720, Bitrate = 2800000 }
            };

            foreach (var v in variants)
            {
                string playlist = Path.Combine(tempDir, $"{v.Height}.m3u8");

                RunProcess(
                    $"-i \"{inputFile}\" " +
                    $"-vf scale=-2:{v.Height} " +
                    "-c:v libx264 -preset fast -crf 23 " +
                    "-c:a aac " +
                    "-hls_time 4 " +
                    "-hls_playlist_type vod " +
                    $"-f hls \"{playlist}\""
                );
            }

            string masterPath = Path.Combine(tempDir, "master.m3u8");

            using (var writer = new StreamWriter(masterPath))
            {
                writer.WriteLine("#EXTM3U");

                foreach (var v in variants)
                {
                    writer.WriteLine(
                        $"#EXT-X-STREAM-INF:BANDWIDTH={v.Bitrate},RESOLUTION=1280x{v.Height}"
                    );

                    writer.WriteLine($"{v.Height}.m3u8");
                }
            }

            string folderKey = $"videos/{Guid.NewGuid()}";

            string url = r2Service.SaveVideo(tempDir, folderKey); //UploadDirectory(tempDir, folderKey);

            return url; //$"{folderKey}/master.m3u8";
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }*/

    public async Task<string> UploadGeneratedVideos(string inputFile)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            await GenerateHlsVariants(inputFile, tempDir);

            string folderKey = $"videos/{Guid.NewGuid()}";

            string url = r2Service.SaveVideo(tempDir, folderKey);

            return url;
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private async Task GenerateHlsVariants(string inputFile, string outputDir)
    {
        Directory.CreateDirectory(outputDir);

        int height = await GetVideoHeight(inputFile);

        var variants = new List<int>();

        if (height >= 720)
            variants.AddRange([360, 480, 720]);
        else if (height >= 480)
            variants.AddRange([360, 480]);
        else if (height >= 360)
            variants.AddRange([360]);

        int count = variants.Count;

        string split = $"[0:v]split={count}";

        for (int i = 0; i < count; i++)
            split += $"[v{i}]";

        split += ";";

        string filters = "";

        for (int i = 0; i < count; i++)
            filters += $"[v{i}]scale=-2:{variants[i]}[v{variants[i]}];";

        string maps = "";
        var streamMaps = new List<string>();

        for (int i = 0; i < count; i++)
        {
            maps += $"-map \"[v{variants[i]}]\" -map a ";
            maps += $"-c:v:{i} libx264 -preset veryfast ";
            maps += $"-b:v:{i} {GetBitrate(variants[i])} ";
            maps += "-g 48 -keyint_min 48 -sc_threshold 0 ";

            streamMaps.Add($"v:{i},a:0");
        }

        string streamMap = string.Join(" ", streamMaps);

        string args =
            $"-i \"{inputFile}\" " +
            $"-filter_complex \"{split}{filters}\" " +
            maps +
            "-c:a aac -b:a 128k " +
            "-f hls " +
            "-hls_time 4 " +
            "-hls_playlist_type vod " +
            "-hls_segment_filename \"" + Path.Combine(outputDir, "stream_%v_%03d.ts") + "\" " +
            "-master_pl_name master.m3u8 " +
            $"-var_stream_map \"{streamMap}\" " +
            $"{Path.Combine(outputDir, "stream_%v.m3u8")}";

        await RunProcess(args);
    }

    private async Task<int> GetVideoHeight(string filePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = @"C:\ffmpeg\bin\ffprobe.exe",
            Arguments = $"-v error -select_streams v:0 -show_entries stream=height -of csv=p=0 \"{filePath}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi) ?? throw new Exception("failed to start ffprobe");

        string output = process.StandardOutput.ReadToEnd();

        await process.WaitForExitAsync();

        return int.Parse(output.Trim());
    }

    private async Task RunProcess(string args)
    {
        var psi = new ProcessStartInfo
        {
            FileName = FfmpegPath,
            Arguments = args,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new Exception("failed to start ffmpeg");

        await process.WaitForExitAsync();

        var error = process.StandardError.ReadToEnd();

        if (process.ExitCode != 0)
            throw new Exception(error);
    }


    private string GetBitrate(int height)
    {
        return height switch
        {
            360 => "800k",
            480 => "1400k",
            720 => "2800k",
            _ => "800k"
        };
    }

    public async Task<int> GetVideoDuration(string filePath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = @"C:\ffmpeg\bin\ffprobe.exe",
            Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{filePath}\"",
            RedirectStandardOutput = true,
            UseShellExecute = false
        };

        using var process = Process.Start(psi) ?? throw new Exception("failed to start ffprobe");

        string output = process.StandardOutput.ReadToEnd();

        await process.WaitForExitAsync();

        double seconds = double.Parse(output, CultureInfo.InvariantCulture);

        return (int)Math.Round(seconds);
    }
}