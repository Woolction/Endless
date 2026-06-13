namespace Application.Features.Images;

public record class ImageDto(
    ImageVariantsDto Variants, int R, int G, int B
);