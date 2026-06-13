namespace Application.Features.Images;

public record class ImageDto(
    ImageVariants Variants, int R, int G, int B
);