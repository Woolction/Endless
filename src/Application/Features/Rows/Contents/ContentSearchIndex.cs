using Application.Features.Images;
using Domain.Entities;

namespace Application.Features.Rows.Contents;

public class ContentSearchIndex
{
    public Guid ContentId { get; set; }
    public Guid? ChannelId { get; set; }
    public Guid CreatorId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ContentType { get; set; }

    public int DurationSeconds { get; set; }
    public float AverageWatchRatio { get; set; }
    public int AverageWatchTimeSeconds { get; set; }

    public string ContentUrl { get; set; } = string.Empty;
    public ImageVariantsDto PreviewPhotoUrl { get; set; } = new();

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public long ViewsCount { get; set; }

    public ContentSearchIndex() { }

    public ContentSearchIndex(Content content, VideoMeta videoMeta)
    {
        ContentId = content.Id;
        ChannelId = content.ChannelId;
        CreatorId = content.CreatorId;

        Title = content.Title;
        Slug = content.Slug;
        Description = content.Description;
        CreatedDate = content.CreatedDate;
        ContentType = (int)content.ContentType;

        if (content.ContentType == Domain.Common.Enums.ContentType.Video)
        {
            ContentUrl = videoMeta.VideoUrl;
            PreviewPhotoUrl = new ImageVariantsDto(
                videoMeta.PhotoBase,
                videoMeta.Small,
                videoMeta.Medium,
                videoMeta.Large
            );

            R = videoMeta.R;
            G = videoMeta.G;
            B = videoMeta.B;

            DurationSeconds = videoMeta.DurationSeconds;
            AverageWatchRatio = videoMeta.AverageWatchRatio;
            AverageWatchTimeSeconds = videoMeta.AverageWatchTimeSeconds;
        }

        ViewsCount = content.ViewsCount;
    }
}