namespace Application.Features.Images;

public record class ImageDto(
    string BaseUrl, List<ImageVariantDto> Variants, int R, int G, int B);