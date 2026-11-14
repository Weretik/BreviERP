using System.Globalization;
using System.Text;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Products;

namespace Catalog.Application.Features.Product.Shared.Overview;

/// <summary>
/// Slug choices for a new product (CAT-009 R-02): the slug from the Ukrainian name, then with the code, then with a
/// number. The caller takes the first one that is free.
/// </summary>
public static class ProductSlugCandidates
{
    public static IReadOnlyList<string> For(IProductSlugGenerator generator, string name, string code)
    {
        // The generator keeps «.» and «_» (sizes such as «5.5»), which the product slug does not allow.
        var baseSlug = Trim(SlugPart(generator.GenerateFromUkrainianName(name)));
        var withCode = Trim(JoinSlug(baseSlug, SlugPart(code)));
        var candidates = new List<string> { baseSlug, withCode };
        for (var n = 2; n <= 50; n++) candidates.Add(JoinSlug(withCode, n.ToString(CultureInfo.InvariantCulture)));
        return candidates.Where(x => x.Length > 0).Distinct().ToList();
    }

    /// <summary>Used when every candidate is taken.</summary>
    public static string Fallback() => "product-" + Guid.NewGuid().ToString("N")[..12];

    private static string JoinSlug(string left, string right)
        => left.Length == 0 ? right : right.Length == 0 ? left : left + "-" + right;

    /// <summary>Lower-case latin letters and digits joined by single hyphens.</summary>
    private static string SlugPart(string value)
    {
        var builder = new StringBuilder();
        foreach (var character in value.ToLowerInvariant())
            builder.Append(character is >= 'a' and <= 'z' or >= '0' and <= '9' ? character : '-');
        return string.Join('-', builder.ToString().Split('-', StringSplitOptions.RemoveEmptyEntries));
    }

    /// <summary>Keeps room for a suffix under the 160-character slug limit.</summary>
    private static string Trim(string slug) => slug.Length <= 140 ? slug : slug[..140].TrimEnd('-');
}
