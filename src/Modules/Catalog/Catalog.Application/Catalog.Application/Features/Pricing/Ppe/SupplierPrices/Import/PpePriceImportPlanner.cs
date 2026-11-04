using Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;
using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Import;

/// <summary>
/// Decides the new supplier price of each product from the file prices, like the client's Apps Script: the discount
/// comes off the supplier price, products with a manual price or not active keep theirs, and Insight's wholesale and
/// retail must be at least 20% above the dealer price, retail not below wholesale.
/// </summary>
public static class PpePriceImportPlanner
{
    private const decimal InsightMinimumMarkup = 1.2m;

    public static IReadOnlyList<PpePriceImportRow> Plan(
        PpePriceSource source,
        decimal discountPercent,
        IReadOnlyList<PpePriceImportProduct> products,
        IReadOnlyList<SupplierFilePrice> filePrices)
    {
        var byUrl = new Dictionary<string, SupplierFilePrice>(StringComparer.Ordinal);
        var bySku = new Dictionary<string, SupplierFilePrice>(StringComparer.Ordinal);
        foreach (var price in filePrices)
        {
            if (SupplierKeys.Url(price.Url) is { } url) byUrl.TryAdd(url, price);
            if (SupplierKeys.Sku(price.Sku) is { } sku) bySku.TryAdd(sku, price);
        }

        return products
            .OrderBy(product => product.Code, StringComparer.OrdinalIgnoreCase)
            .Select(product => PlanProduct(source, discountPercent, product, Find(product, byUrl, bySku)))
            .ToList();
    }

    public static decimal ApplyDiscount(decimal price, decimal discountPercent)
        => decimal.Round(price * (100m - discountPercent) / 100m, 2, MidpointRounding.AwayFromZero);

    private static SupplierFilePrice? Find(
        PpePriceImportProduct product,
        IReadOnlyDictionary<string, SupplierFilePrice> byUrl,
        IReadOnlyDictionary<string, SupplierFilePrice> bySku)
    {
        if (SupplierKeys.Url(product.SupplierUrl) is { } url && byUrl.TryGetValue(url, out var byLink)) return byLink;
        return SupplierKeys.Sku(product.SupplierSku) is { } sku && bySku.TryGetValue(sku, out var byArticle)
            ? byArticle
            : null;
    }

    private static string NotFoundMessage(PpePriceSource source, PpePriceImportProduct product) => source.Method switch
    {
        PpePriceSourceMethod.Website => string.IsNullOrWhiteSpace(product.SupplierUrl)
            ? "У товару немає посилання на сторінку постачальника."
            : "Сторінку постачальника не перевірено.",
        PpePriceSourceMethod.GoogleSheets => string.IsNullOrWhiteSpace(product.SupplierSku)
            ? "У товару немає артикулу постачальника."
            : $"Артикул {product.SupplierSku} не знайдено в прайсах {source.Title}.",
        _ => product.SupplierUrl is null && product.SupplierSku is null
            ? "У товару немає посилання й артикулу постачальника."
            : "Товар не знайдено у файлі за посиланням або артикулом постачальника."
    };

    private static PpePriceImportRow PlanProduct(
        PpePriceSource source, decimal discountPercent, PpePriceImportProduct product, SupplierFilePrice? found)
    {
        PpePriceImportRow Row(PpePriceImportStatus status, string? message = null, decimal? price = null,
            decimal? wholesale = null, decimal? retail = null)
            => new(product.ProductId, product.Code, product.Name, status, product.CurrentPrice, price,
                product.CurrentWholesale, wholesale, product.CurrentRetail, retail, message);

        if (product.AllowManualPrice) return Row(PpePriceImportStatus.ManualPrice, "Увімкнено «Дозволити ручну ціну».");
        if (!product.IsActive) return Row(PpePriceImportStatus.Inactive, "Товар неактивний.");
        if (found is null) return Row(PpePriceImportStatus.NotInFile, NotFoundMessage(source, product));
        if (found.Problem is not null) return Row(PpePriceImportStatus.Invalid, found.Problem);
        if (found.Price <= 0) return Row(PpePriceImportStatus.Invalid, "У файлі немає ціни.");

        var price = ApplyDiscount(found.Price, discountPercent);
        decimal? wholesale = product.CurrentWholesale;
        decimal? retail = product.CurrentRetail;
        if (source.FileKind == PpePriceFileKind.InsightYml)
        {
            if (found.Wholesale is not > 0 || found.Retail is not > 0)
                return Row(PpePriceImportStatus.Invalid, "Не в усіх трьох XML є ціна.");
            if (found.Wholesale < found.Price * InsightMinimumMarkup || found.Retail < found.Price * InsightMinimumMarkup)
                return Row(PpePriceImportStatus.Invalid, "Оптова або роздрібна ціна нижча ніж дилерська +20%.");
            if (found.Retail < found.Wholesale)
                return Row(PpePriceImportStatus.Invalid, "Роздрібна ціна нижча за оптову.");
            wholesale = found.Wholesale;
            retail = found.Retail;
        }

        var unchanged = price == product.CurrentPrice
                        && wholesale == product.CurrentWholesale
                        && retail == product.CurrentRetail;
        return Row(unchanged ? PpePriceImportStatus.Unchanged : PpePriceImportStatus.Changed, null, price, wholesale, retail);
    }
}
