using Domain.Entities;

namespace Domain.Rows.Channels;

public class ChannelSearchIndex
{
    public Guid ChannelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? AvatarPhotoUrl { get; set; }

    public long TotalViews { get; set; }
    public long TotalLikes { get; set; }

    public ChannelSearchIndex() {}

    public ChannelSearchIndex(Channel channel)
    {
        ChannelId = channel.Id;

        Name = channel.Name;
        Slug = channel.Slug;
        Description = channel.Description;
        CreatedDate = channel.CreatedDate;
        AvatarPhotoUrl = channel.AvatarPhotoUrl;

        TotalLikes = channel.TotalLikes;
        TotalViews = channel.TotalViews;
    }
}