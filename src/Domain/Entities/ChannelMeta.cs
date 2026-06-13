namespace Domain.Entities;

public class ChannelMeta
{
    public Guid ChannelId { get; set; }
    public Channel? Channel { get; set; }

    public Guid ImageId { get; set; }
    public Image? Image { get; set; }

    public string IconBase { get; set; } = "/storage/images/channel-icons";
    public string Small { get; set; } = string.Empty;
    public string? Medium { get; set; }
    public string? Large { get; set; }

    public int R { get; set; }
    public int G { get; set; }
    public int B { get; set; }

    public void SetImage(Image image)
    {
        Image = image;
    }
}