namespace Domain.Entities;

public class ImageVariant
{
    public Guid ImageId { get; set; }
    public Image? Image { get; set; }
    
    public required string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public void SetData(string url, int width, int height)
    {
        Url = url;
        Width = width;
        Height = height;
    }
}