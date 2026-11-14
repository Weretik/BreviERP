
namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices.Sources;

/// <summary>
/// A price found in the supplier's files. <paramref name="Wholesale"/> and <paramref name="Retail"/> come only from
/// Insight's three lists; <paramref name="Problem"/> marks an item whose price cannot be used, with its reason.
/// </summary>
public sealed record SupplierFilePrice(
    string? Sku,
    string? Url,
    string? Name,
    decimal Price,
    decimal? Wholesale,
    decimal? Retail,
    string? Problem);
