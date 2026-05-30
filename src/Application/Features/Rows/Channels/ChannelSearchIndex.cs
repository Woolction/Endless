using Application.Features.Rows.Contents;
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

    public PhotoVariants IconVariants { get; set; } = new();

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public ChannelSearchIndex() { }

    public ChannelSearchIndex(Channel channel, ChannelMeta channelMeta)
    {
        ChannelId = channel.Id;

        Name = channel.Name;
        Slug = channel.Slug;
        Description = channel.Description;
        CreatedDate = channel.CreatedDate;

        IconVariants = new PhotoVariants(
            channelMeta.IconBase,
            channelMeta.Small,
            channelMeta.Medium,
            channelMeta.Large
        );

        R = channelMeta.R;
        G = channelMeta.G;
        B = channelMeta.B;

        TotalLikes = channel.TotalLikes;
        TotalViews = channel.TotalViews;
    }
}