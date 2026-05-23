using Domain.Rows.Contents;

namespace Application.Dtos;

public record class PreviewPhotoDto(
    PreviewPhotoVariants Url, int R, int G, int B
);