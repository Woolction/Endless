using Domain.Common;

namespace Domain.Entities;

public class ChannelOwner
{
    public Guid OwnerId { get; set; }
    public User? Owner { get; set; }

    public Guid ChannelId { get; set; }
    public Channel? Channel { get; set; }

    public DateTime OwnedDate { get; set; }
    public ChannelOwnerRole OwnerRole { get; set; } = ChannelOwnerRole.ContentEditor;
}
