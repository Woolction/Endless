using System.Text.RegularExpressions;

namespace Application.Utilities;

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

    public static string Sanitize(this string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // remove control characters
        text = ControlCharactersRegex().Replace(text, "");

        // remove blocs [...]
        text = BlocsRegex().Replace(text, "");

        // space compression
        text = SpaceRegex().Replace(text, " ").Trim();

        return text;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRegex();
    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex SymbolRegex();
    [GeneratedRegex(@"\[[^\]]*\]")]
    private static partial Regex BlocsRegex();
    [GeneratedRegex(@"[\u0000-\u001F\u007F]")]
    private static partial Regex ControlCharactersRegex();
}