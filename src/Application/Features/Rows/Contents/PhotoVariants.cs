namespace Application.Features.Rows.Contents;

public class PhotoVariants
{
    public string BaseUrl { get; init; } = string.Empty;
    public string Small { get; init; } = string.Empty;
    public string? Medium { get; init; }
    public string? Large { get; init; }

    public PhotoVariants(string baseUrl, string small, string? medium, string? large)
    {
        BaseUrl = baseUrl;
        Small = small;
        Medium = medium;
        Large = large;
    }

    public PhotoVariants()
    {

    }
}