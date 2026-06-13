namespace Application.Features.Images;

public class ImageVariantsDto
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Small { get; init; } = string.Empty;
    public string? Medium { get; init; }
    public string? Large { get; init; }

    public List<ImageVariantDto> Variants { get; set; } = [];

    public ImageVariantsDto(string baseUrl, string small, string? medium, string? large)
    {
        BaseUrl = baseUrl;
        Small = small;
        Medium = medium;
        Large = large;
    }

    public ImageVariantsDto(string baseUrl, List<ImageVariantDto> variants)
    {
        BaseUrl = baseUrl;
        Variants = variants;
    }

    public ImageVariantsDto()
    {

    }
}