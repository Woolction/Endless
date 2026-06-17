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

    // Image
    public Guid ImageId { get; set; }
    public Image Image { get; set; } = new() { BaseUrl = "/storage/images/content" };


    public void SetVideo(string videoUrl, int durationSeconds)
    {
        VideoUrl = videoUrl;
        DurationSeconds = durationSeconds;
    }
}