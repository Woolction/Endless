namespace Domain.Entities;

public class ChannelMeta
{
    public Guid ChannelId { get; set; }
    public Channel? Channel { get; set; }

    public Guid ImageId { get; set; }
    public Image Image { get; set; } = new() { BaseUrl = "/storage/images/channel" };
}