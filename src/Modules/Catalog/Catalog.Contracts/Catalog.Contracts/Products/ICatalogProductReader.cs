namespace Catalog.Contracts.Products;

/// <summary>Products of «Товари» for other modules (КП, workshop, site texts, imports).</summary>
public interface ICatalogProductReader
{
    /// <summary>One page of products of a kind, by code; <paramref name="search"/> matches code and name.</summary>
    Task<CatalogProductPage> PageAsync(CatalogProductKind kind, string? search, int page, int pageSize,
        CancellationToken cancellationToken);

    /// <summary>Every product of a kind (a few hundred) matching <paramref name="search"/>, by code.</summary>
    Task<IReadOnlyList<CatalogProduct>> ListAsync(CatalogProductKind kind, string? search,
        CancellationToken cancellationToken);

    Task<CatalogProduct?> FindAsync(int productId, CancellationToken cancellationToken);

    /// <summary>The photos of the card tab «Фото» that exist in the library, main first.</summary>
    Task<IReadOnlyList<CatalogPhoto>> PhotosAsync(int productId, CancellationToken cancellationToken);
}
