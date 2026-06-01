using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp;

namespace Domain.Entities;

public class ChannelMeta
{
    public Guid ChannelId { get; set; }
    public Channel? Channel { get; set; }

    public string IconBase { get; set; } = "/storage/images/channel-icons";
    public string Small { get; set; } = string.Empty;
    public string? Medium { get; set; }
    public string? Large { get; set; }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public void SetPhoto(string iconBase, string small, string? medium, string? large)
    {
        IconBase = iconBase;
        Small = small;
        Medium = medium;
        Large = large;
    }

    public async Task SetAverageColor(string photoUrl, CancellationToken token = default)
    {
        photoUrl = Path.Combine(IconBase, photoUrl);

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

        Console.WriteLine($"rgb({R} {G} {B}) for {Path.GetFileNameWithoutExtension(photoUrl)} and Channel Id: {ChannelId}");
    }
}