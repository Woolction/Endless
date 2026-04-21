using Domain.Common;
using Domain.Entities;

namespace Domain.Rows.Contents;

public class ContentSearchIndex
{
    public Guid ContentId { get; set; }
    public Guid? ChannelId { get; set; }
    public Guid CreatorId { get; set; }

    public string Title { get; set; } = string.Empty;
    public Guid Slug { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public ContentType ContentType { get; set; }

    public int DurationSeconds { get; set; }
    public float AverageWatchRatio { get; set; }
    public int AverageWatchTimeSeconds { get; set; }

    public string? ContentUrl { get; set; }
    public string? PrewievPhotoUrl { get; set; }

    public long ViewsCount { get; set; }

    public ContentSearchIndex() {}

    public ContentSearchIndex(Content content)
    {
        ContentId = content.Id;
        ChannelId = content.ChannelId;
        CreatorId = content.CreatorId;

        Title = content.Title;
        Slug = content.Slug;
        Description = content.Description;
        CreatedDate = content.CreatedDate;
        ContentType = content.ContentType;

        if (ContentType == ContentType.Video)
        {
            VideoMetaData? videoMeta = content.VideoMeta;

            if (videoMeta != null)
            {
                DurationSeconds = videoMeta.DurationSeconds;
                AverageWatchRatio = videoMeta.AverageWatchRatio;
                AverageWatchTimeSeconds = videoMeta.AverageWatchTimeSeconds;
            }
        }

        ContentUrl = content.ContentUrl;
        PrewievPhotoUrl = content.PrewievPhotoUrl;

        ViewsCount = content.ViewsCount;
    }
}