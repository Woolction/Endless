namespace Domain.Rows.Contents;

public class PreviewPhotoVariants
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Small { get; init; } = string.Empty;
    public string? Medium { get; init; }
    public string? Large { get; init; }

    public PreviewPhotoVariants(string baseUrl, string small, string? medium, string? large)
    {
        BaseUrl = baseUrl;
        Small = small;
        Medium = medium;
        Large = large;
    }

    public PreviewPhotoVariants()
    {

    }
}