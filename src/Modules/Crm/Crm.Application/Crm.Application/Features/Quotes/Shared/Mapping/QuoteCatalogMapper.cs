using Crm.Domain.Quotes;
using Crm.Application.Features.Quotes.Shared;
using Crm.Application.Features.Quotes.Shared.Catalog;
using Crm.Application.Abstractions.Quotes;
using Crm.Domain.Quotes.Enums;
using Crm.Application.Abstractions.Quotes.Products;

namespace Crm.Application.Features.Quotes.Shared.Mapping;

/// <summary>Catalog rows for the picker; a problem is shown only when a row cannot be added.</summary>
internal static class QuoteCatalogMapper
{
    internal static QuoteCatalogPageResponse ToResponse(this QuoteCatalogPage page) =>
        new(page.Items.Select(ToResponse).ToArray(), page.Page, page.PageSize, page.Total);

    public static QuoteCatalogProductResponse ToResponse(this QuoteCatalogProduct product) => new(
        product.ProductId, product.Kind.ToString(), product.Code, product.Name, product.Season, product.PhotoUrl,
        product.Description, ToResponse(product.Prices), ProductProblem(product),
        product.Variants.Select(ToResponse).ToArray());

    private static QuoteCatalogVariantResponse ToResponse(QuoteCatalogVariant variant) => new(
        variant.ArticleId, variant.Sku, variant.Title, variant.FabricInfo, ToResponse(variant.Prices),
        !variant.IsActive ? "Варіант вимкнений."
        : variant.Prices is null ? Problems(variant.Problems, "Немає опублікованої ціни.")
        : null,
        variant.FabricId);

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
