using System.Text.RegularExpressions;

namespace Backend.API.Managers;

public static partial class TextManager
{
    public static string GenerateSlug(this string text)
    {
        string slug = text.ToLowerInvariant();

        slug = SpaceRegex().Replace(slug, "-");

        slug = SymbolRegex().Replace(slug, "");

        slug = slug.Trim("-").ToString();

        return slug;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();
    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex SymbolRegex();
}