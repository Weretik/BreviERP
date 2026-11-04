using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Catalog;

/// <summary>
/// Catalog products as a КП needs them: texts, current published prices and fabric variants. Implemented in the
/// host over the Catalog, Reference and Media queries; Crm never reads their tables.
/// </summary>
public interface IQuoteCatalogReader
{
    /// <param name="kind">Sewing or Ppe.</param>
    Task<QuoteCatalogPage> SearchAsync(QuoteItemKind kind, string? search, int page, int pageSize,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<int, QuoteCatalogProduct>> GetAsync(IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken);
}

/// <summary>Prices for 1–9, 10–40 and from 41 pieces.</summary>
public sealed record QuoteTierPrices(decimal Retail, decimal Wholesale, decimal Large);

/// <summary>
/// A sewing model or PPE product. <paramref name="Prices"/> is the PPE price or the lowest variant prices of a model;
/// <paramref name="Problems"/> are why a price is missing.
/// </summary>
public sealed record QuoteCatalogProduct(
    int ProductId,
    QuoteItemKind Kind,
    string? Code,
    string Name,
    string? Season,
    bool IsActive,
    string? SiteUrl,
    string? Description,
    string? PhotoUrl,
    string? SupplierUrl,
    QuoteTierPrices? Prices,
    IReadOnlyList<string> Problems,
    IReadOnlyList<QuoteCatalogVariant> Variants);

/// <summary>A fabric variant of a sewing model with its published prices and client texts.</summary>
public sealed record QuoteCatalogVariant(
    long ArticleId,
    string Sku,
    bool IsActive,
    int? FabricId,
    string Title,
    string? Details,
    string? FabricInfo,
    QuoteTierPrices? Prices,
    IReadOnlyList<string> Problems);

public sealed record QuoteCatalogPage(IReadOnlyList<QuoteCatalogProduct> Items, int Page, int PageSize, int Total);
