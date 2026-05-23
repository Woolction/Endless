using Domain.Rows.Contents;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Domain.Entities;

public class VideoMetaData
{
    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    //Video
    public string VideoUrl { get; set; } = string.Empty;

    public int DurationSeconds { get; set; }
    public int AverageWatchTimeSeconds { get; set; }
    public float AverageWatchRatio { get; set; }

    //Photo
    public string BaseUrl { get; set; } = string.Empty;
    public string Small { get; set; } = string.Empty;
    public string? Medium { get; set; }
    public string? Large { get; set; }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public void SetVideo(string videoUrl, int durationSeconds)
    {
        VideoUrl = videoUrl;
        DurationSeconds = durationSeconds;
    }

    public void SetPhoto(PreviewPhotoVariants variants)
    {
        BaseUrl = variants.BaseUrl;
        Small = variants.Small;
        Medium = variants.Medium;
        Large = variants.Large;
    }

    public PreviewPhotoVariants GetPhotoVariants()
    {
        return new(BaseUrl, Small, Medium, Large);
    }

    public async Task SetAverageColor(string photoUrl, CancellationToken token = default)
    {
        using Image<Rgba32> image =
            await Image.LoadAsync<Rgba32>(photoUrl, token);

        image.Mutate(x => x.Resize(64, 64));

        long r = 0;
        long g = 0;
        long b = 0;

        int count = 0;

        for (int y = 0; y < image.Height; y++)
        {
            Span<Rgba32> row = image.DangerousGetPixelRowMemory(y).Span;

            for (int x = 0; x < image.Width; x++)
            {
                Rgba32 pixel = row[x];

                r += pixel.R;
                g += pixel.G;
                b += pixel.B;

                count++;
            }
        }

        R = (int)(r / count);
        G = (int)(g / count);
        B = (int)(b / count);

        Console.WriteLine($"rgb({R} {G} {B}) for {Path.GetFileNameWithoutExtension(photoUrl)} and ContentId: {ContentId}");
    }

    public async Task<(int R, int G, int B)> GetAverageColor(string photoUrl, CancellationToken token = default)
    {
        using Image<Rgba32> image =
            await Image.LoadAsync<Rgba32>(photoUrl, token);

        image.Mutate(x => x.Resize(64, 64));

        long r = 0;
        long g = 0;
        long b = 0;

        int count = 0;

        for (int y = 0; y < image.Height; y++)
        {
            Span<Rgba32> row = image.DangerousGetPixelRowMemory(y).Span;

            for (int x = 0; x < image.Width; x++)
            {
                Rgba32 pixel = row[x];

                r += pixel.R;
                g += pixel.G;
                b += pixel.B;

                count++;
            }
        }

        return (
            (int)(r / count),
            (int)(g / count),
            (int)(b / count)
        );
    }
}