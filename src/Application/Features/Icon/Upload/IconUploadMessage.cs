using Domain.Common.Enums;

namespace Application.Features.Icon.Upload;

public record class IconUploadMessage(
    Guid Id, IconType Type, string Slug, string PhotoPath
);