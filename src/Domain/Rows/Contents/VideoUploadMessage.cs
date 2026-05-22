using Domain.Common.Enums;
using Domain.Entities;

namespace Domain.Rows.Contents;

public record VideoUploadMessage(
    Guid ContentId, string Slug, string? VideoPath, string? PhotoPath);