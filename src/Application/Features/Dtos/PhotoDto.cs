using Application.Features.Images;

namespace Application.Features.Dtos;

public record class PhotoDto(
    ImageVariants Variants, int R, int G, int B
);