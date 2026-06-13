namespace Application.Features.Images;

public class ImageVariants
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Small { get; init; } = string.Empty;
    public string? Medium { get; init; }
    public string? Large { get; init; }

    public List<ImageVariant> Variants { get; set; } = [];

    public ImageVariants(string baseUrl, string small, string? medium, string? large)
    {
        BaseUrl = baseUrl;
        Small = small;
        Medium = medium;
        Large = large;
    }

    public ImageVariants(List<ImageVariant> variants)
    {
        Variants = variants;
    }

    public ImageVariants()
    {

    }
}