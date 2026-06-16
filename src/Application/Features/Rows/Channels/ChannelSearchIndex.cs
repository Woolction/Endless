using Application.Features.Images;
using Domain.Entities;

namespace Application.Features.Rows.Channels;

public class ChannelSearchIndex
{
    public Guid ChannelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }

    public ImageVariantsDto Icon { get; set; } = new ();

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public ChannelSearchIndex() { }

    public ChannelSearchIndex(Channel channel, Image image, List<ImageVariant> variants)
    {
        ChannelId = channel.Id;

        Name = channel.Name;
        Slug = channel.Slug;
        Description = channel.Description;
        CreatedDate = channel.CreatedDate;

        for (int i = 0; i < variants.Count; i++)
        {
            var variant = variants[i];

            Icon.Variants.Add(new ImageVariantDto(
                variant.Url, variant.Width, variant.Height));
        }

        R = image.R;
        G = image.G;
        B = image.B;

        TotalLikes = channel.TotalLikes;
        TotalViews = channel.TotalViews;
    }
}