namespace Application.Features.Images;

public class ImageVariantsDto
{
    public string BaseUrl { get; init; } = string.Empty;

    public List<ImageVariantDto> Variants { get; set; } = [];

    public ImageVariantsDto(string baseUrl, List<ImageVariantDto> variants)
    {
        BaseUrl = baseUrl;
        Variants = variants;
    }

    public ImageVariantsDto()
    {

    }
}