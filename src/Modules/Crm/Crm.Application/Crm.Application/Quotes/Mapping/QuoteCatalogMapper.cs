using Crm.Application.Quotes.Catalog;
using Crm.Application.Quotes.Contracts;
using Crm.Domain.Quotes;

namespace Crm.Application.Quotes.Mapping;

/// <summary>Catalog rows for the picker; a problem is shown only when a row cannot be added.</summary>
internal static class QuoteCatalogMapper
{
    public static QuoteCatalogPageResponse ToResponse(this QuoteCatalogPage page) =>
        new(page.Items.Select(ToResponse).ToArray(), page.Page, page.PageSize, page.Total);

    private static QuoteCatalogProductResponse ToResponse(QuoteCatalogProduct product) => new(
        product.ProductId, product.Kind.ToString(), product.Code, product.Name, product.Season, product.PhotoUrl,
        product.Description, ToResponse(product.Prices), ProductProblem(product),
        product.Variants.Select(ToResponse).ToArray());

    private static QuoteCatalogVariantResponse ToResponse(QuoteCatalogVariant variant) => new(
        variant.ArticleId, variant.Sku, variant.Title, variant.FabricInfo, ToResponse(variant.Prices),
        !variant.IsActive ? "Варіант вимкнений."
        : variant.Prices is null ? Problems(variant.Problems, "Немає опублікованої ціни.")
        : null);

    private static string? ProductProblem(QuoteCatalogProduct product)
    {
        if (!product.IsActive) return "Товар вимкнений.";
        if (product.Kind == QuoteItemKind.Ppe)
            return product.Prices is null ? Problems(product.Problems, "Немає ціни.") : null;
        return product.Variants.Any(x => x.IsActive && x.Prices is not null)
            ? null
            : "Немає варіантів тканини з опублікованою ціною.";
    }

    private static QuoteTierPricesResponse? ToResponse(QuoteTierPrices? prices) =>
        prices is null ? null : new QuoteTierPricesResponse(prices.Retail, prices.Wholesale, prices.Large);

    private static string Problems(IReadOnlyList<string> problems, string fallback) =>
        problems.Count == 0 ? fallback : string.Join(", ", problems);
}
