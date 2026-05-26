using Domain.Rows.Contents;

namespace Application.Dtos;

public record class PhotoDto(
    PhotoVariants Variants, int R, int G, int B
);