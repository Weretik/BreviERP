using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Catalog;

/// <summary>
/// R-29: an item of a Catalog product shows the product's main photo when it has none of its own or still has a
/// photo of the old site (brevi.com.ua, the imported КП). A photo picked with «Змінити фото» stays.
/// </summary>
public static class QuoteItemPhotos
{
    private static readonly string[] OldSiteHosts = ["brevi.com.ua", "www.brevi.com.ua"];

    public static bool NeedsCatalogPhoto(string? photoUrl) =>
        string.IsNullOrWhiteSpace(photoUrl)
        || (Uri.TryCreate(photoUrl, UriKind.Absolute, out var uri)
            && OldSiteHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase));

    /// <summary>The main photos of the products whose items need one, by product.</summary>
    public static async Task<IReadOnlyDictionary<int, string>> CatalogPhotosAsync(
        IEnumerable<(int? ProductId, string? PhotoUrl)> items, IQuoteCatalogReader catalog,
        CancellationToken cancellationToken)
    {
        var ids = items.Where(x => x.ProductId is not null && NeedsCatalogPhoto(x.PhotoUrl))
            .Select(x => x.ProductId!.Value).Distinct().ToArray();
        if (ids.Length == 0) return new Dictionary<int, string>();
        var products = await catalog.GetAsync(ids, cancellationToken);
        return products.Where(x => !string.IsNullOrWhiteSpace(x.Value.PhotoUrl))
            .ToDictionary(x => x.Key, x => x.Value.PhotoUrl!);
    }

    /// <summary>The photo the item shows: its own, or the product's main photo when <see cref="NeedsCatalogPhoto"/>.</summary>
    public static string? Resolve(int? productId, string? photoUrl, IReadOnlyDictionary<int, string> catalogPhotos) =>
        productId is { } id && NeedsCatalogPhoto(photoUrl) && catalogPhotos.TryGetValue(id, out var main)
            ? main
            : photoUrl;

    public static Task<IReadOnlyDictionary<int, string>> CatalogPhotosAsync(Quote quote, IQuoteCatalogReader catalog,
        CancellationToken cancellationToken) =>
        CatalogPhotosAsync(quote.Items.Select(x => (x.ProductId, x.PhotoUrl)), catalog, cancellationToken);
}
