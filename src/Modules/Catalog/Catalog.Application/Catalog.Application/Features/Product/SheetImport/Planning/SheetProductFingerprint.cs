using System.Globalization;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.SheetImport.Planning;

/// <summary>
/// Every field the import writes, as text, to tell an unchanged product from a changed one. Review marks are
/// compared by state only, so a model reviewed in Brevi is not rewritten for the reviewer name or time.
/// </summary>
public static class SheetProductFingerprint
{
    public static string Of(ProductEntity product)
    {
        var p = product.CatalogProfile;
        var skus = product.Articles.ToDictionary(x => x.Id, x => x.SaleSku);
        var parts = new List<string?>
        {
            product.Name, product.RuName, product.Code,
            Text(p?.SectionId), p?.Season, Text(p?.IsActive), Text(p?.IsSourceReviewed), p?.SourceReviewDate,
            p?.WebsiteUrl, p?.ShortDescription,
            Text(p?.SupplierId?.Value), p?.SupplierSku, p?.SupplierProductUrl, Text(p?.SupplierPrice),
            Text(p?.AllowManualPrice), p?.PricingRuleCode, p?.Unit,
        };
        parts.AddRange(product.Articles.OrderBy(x => x.SortOrder).ThenBy(x => x.SaleSku, StringComparer.Ordinal)
            .Select(x => $"A|{x.SaleSku}|{Text(x.IsActive)}|{x.SortOrder}"));
        parts.AddRange(product.MaterialLines.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)
            .Select(x => string.Join('|', "M", x.ArticleId is { } a ? skus.GetValueOrDefault(a) : "", x.ReferenceKey,
                Text(x.Quantity), x.Unit, Text(x.IsRequired), x.SortOrder, Text(x.IsReviewed), x.Comment)));
        parts.AddRange(product.OperationLines.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)
            .Select(x => string.Join('|', "O", x.OperationId.Value, Text(x.Quantity), Text(x.ManualMinutes),
                x.SortOrder, Text(x.IsReviewed), x.Comment)));
        return string.Join('\n', parts.Select(x => x ?? "∅"));
    }

    private static string? Text(decimal? value) => value?.ToString("G29", CultureInfo.InvariantCulture);

    private static string? Text(int? value) => value?.ToString(CultureInfo.InvariantCulture);

    private static string? Text(bool? value) => value?.ToString();
}
