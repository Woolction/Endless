using Domain.Common.Enums;

namespace Domain.Rows.Icon.Upload;

public record class IconUploadMessage(
    Guid Id, IconType Type, string Slug, string PhotoPath
);