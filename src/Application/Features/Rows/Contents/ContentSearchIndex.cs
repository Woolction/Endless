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
    public ImageDto? Preview { get; set; }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public long ViewsCount { get; set; }

    public ContentSearchIndex() { }

    public ContentSearchIndex(Content content, VideoMeta videoMeta, Image image, List<ImageVariant> variants)
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

            Preview = new(
                image.BaseUrl, variants
                    .Select(v => new ImageVariantDto(v.Url, v.Width, v.Height))
                    .ToList(),
                image.R, image.G, image.B);

            for (int i = 0; i < variants.Count; i++)
            {
                var variant = variants[i];

                Preview.Variants.Add(new ImageVariantDto(
                    variant.Url, variant.Width, variant.Height));
            }

            R = image.R;
            G = image.G;
            B = image.B;

            DurationSeconds = videoMeta.DurationSeconds;
            AverageWatchRatio = videoMeta.AverageWatchRatio;
            AverageWatchTimeSeconds = videoMeta.AverageWatchTimeSeconds;
        }

        ViewsCount = content.ViewsCount;
    }
}