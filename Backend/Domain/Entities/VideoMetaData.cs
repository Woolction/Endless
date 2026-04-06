namespace Domain.Entities;

public class VideoMetaData
{
    public Guid ContentId { get; set; }
    public Content? Content { get; set; }

    public int DurationSeconds { get; set; }
    public int AverageWatchTimeSeconds { get; set; }
    public float AverageWatchRatio { get; set; }
}