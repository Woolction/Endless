namespace Application.Features.Images;

public class ImageVariantDto
{
    public string Url { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }

    public ImageVariantDto(string url, int width, int height)
    {
        Url = url;
        Width = width;
        Height = height;
    }
}