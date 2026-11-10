using System.Text;

namespace Content.Domain.ProductPages;

/// <summary>Limits and the slug rule of a product page (CNT-002 R-03, R-04).</summary>
public static class ProductPageRules
{
    public const string Ukrainian = "uk";
    public const string Russian = "ru";
    public static readonly IReadOnlyList<string> Languages = [Ukrainian, Russian];

    public const int SlugMinLength = 3;
    public const int SlugMaxLength = 120;
    public const int NameMaxLength = 200;
    public const int SeoTitleMaxLength = 120;
    public const int SeoDescriptionMaxLength = 320;
    public const int FactLabelMaxLength = 80;
    public const int FactTextMaxLength = 1_000;
    public const int MaxFacts = 20;
    public const int MaxBlocks = 200;
    public const int DescriptionMaxLength = 20_000;
    public const int SpecTitleMaxLength = 120;
    public const int SpecNameMaxLength = 120;
    public const int SpecValueMaxLength = 500;
    public const int MaxSpecGroups = 20;
    public const int MaxSpecRows = 60;
    public const int UpdatedByMaxLength = 256;

    private static readonly Dictionary<char, string> Latin = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "h", ['ґ'] = "g", ['д'] = "d", ['е'] = "e", ['є'] = "ye",
        ['ж'] = "zh", ['з'] = "z", ['и'] = "y", ['і'] = "i", ['ї'] = "yi", ['й'] = "y", ['к'] = "k", ['л'] = "l",
        ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['у'] = "u",
        ['ф'] = "f", ['х'] = "kh", ['ц'] = "ts", ['ч'] = "ch", ['ш'] = "sh", ['щ'] = "shch", ['ю'] = "yu", ['я'] = "ya",
        ['ь'] = "", ['ъ'] = "", ['\''] = "", ['’'] = "", ['ʼ'] = "", ['ы'] = "y", ['э'] = "e", ['ё'] = "yo",
    };

    public static bool IsLanguage(string? language) => language is Ukrainian or Russian;

    /// <summary>Lower-case Latin letters, digits and single hyphens, 3–120 characters, no hyphen at the ends.</summary>
    public static bool IsValidSlug(string? slug)
    {
        if (slug is null || slug.Length is < SlugMinLength or > SlugMaxLength) return false;
        if (slug[0] == '-' || slug[^1] == '-' || slug.Contains("--", StringComparison.Ordinal)) return false;
        return slug.All(c => c is >= 'a' and <= 'z' or >= '0' and <= '9' or '-');
    }

    /// <summary>A slug from a name: Cyrillic is written in Latin, anything else becomes a hyphen. Empty when nothing is left.</summary>
    public static string SlugFrom(string? name)
    {
        var slug = new StringBuilder();
        foreach (var c in (name ?? string.Empty).Trim().ToLowerInvariant())
        {
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9') slug.Append(c);
            else if (Latin.TryGetValue(c, out var latin)) slug.Append(latin);
            else if (slug.Length > 0 && slug[^1] != '-') slug.Append('-');
        }

        var text = slug.ToString().Trim('-');
        return text.Length > SlugMaxLength ? text[..SlugMaxLength].Trim('-') : text;
    }

    /// <summary>Trims and joins inner white space; null for an empty value.</summary>
    public static string? Clean(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
