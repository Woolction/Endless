namespace Domain.Rows.Contents.Video.Upload;

public record class VideoUploadMessage(
    Guid ContentId, string Slug, string? VideoPath, string? PhotoPath
);