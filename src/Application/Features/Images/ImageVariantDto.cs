namespace Application.Features.Images;

public class ImageVariantDto
{
    public required string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}