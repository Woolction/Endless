using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using Application.Interfaces.Services;
using SixLabors.ImageSharp.Advanced;
using Microsoft.Extensions.Logging;
using Application.Features.Images;
using SixLabors.ImageSharp;

namespace Media.Services;

public class ImageAnalyzer : IImageAnalyzer
{
    private readonly ILogger<ImageAnalyzer> logger;
    public ImageAnalyzer(ILogger<ImageAnalyzer> logger)
    {
        this.logger = logger;
    }

    public async Task SetAverageColor(string photoUrl, Action<int, int, int> setColor, CancellationToken token = default)
    {
        photoUrl = Path.Combine(photoUrl);

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

        int R = (int)(r / count);
        int G = (int)(g / count);
        int B = (int)(b / count);

        setColor(R, G, B);

        logger.LogInformation("rgb({R} {G} {B}) for {path}",
            R, G, B, Path.GetFileNameWithoutExtension(photoUrl));
    }

    public async Task<ImageVariantsDto> GenerateImageVariantsDto(string photoPath, string folder, (int w, int h)[] sizes, int quality = 80, CancellationToken token = default)
    {
        using var image = await Image.LoadAsync(photoPath, token);

        var useableSizes = new List<(int w, int h)>();

        for (int i = 0; i < sizes.Length; i++)
        {
            var (w, h) = sizes[i];

            if (image.Width >= w && image.Height >= h)
            {
                useableSizes.Add((w, h));
            }
        }

        for (int i = 0; i < useableSizes.Count; i++)
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
                Quality = quality
            }, token);
        }

        return new ImageVariantsDto(
            folder, "640x360.webp", "960x540.webp", "1280x720.webp"
        );
    }
}