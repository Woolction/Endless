namespace Domain.Entities;

public class VideoMeta
{
    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    // video
    public string VideoUrl { get; set; } = string.Empty;

    public int DurationSeconds { get; set; }
    public int AverageWatchTimeSeconds { get; set; }
    public float AverageWatchRatio { get; set; }

    // photo
    public string PhotoBase { get; set; } = "/storage/content-previews";
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

    public void SetPhoto(string photoBase, string small, string? medium, string? large)
    {
        PhotoBase = photoBase;
        Small = small;
        Medium = medium;
        Large = large;
    }

    public void SetColor(int r, int g, int b)
    {
        R = r;
        G = g;
        B = b;
    }
}