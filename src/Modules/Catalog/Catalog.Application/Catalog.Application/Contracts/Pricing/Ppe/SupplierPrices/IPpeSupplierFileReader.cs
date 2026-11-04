using Catalog.Application.Features.Pricing.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Contracts.Pricing.Ppe.SupplierPrices;

/// <summary>An uploaded supplier price file; the stream is read once.</summary>
public sealed record SupplierPriceFile(string Name, Stream Content);

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

/// <summary>Reads supplier price files; a file that cannot be read returns an invalid result with the reason.</summary>
public interface IPpeSupplierFileReader
{
    Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadAsync(
        PpePriceFileKind kind, IReadOnlyList<SupplierPriceFile> files, CancellationToken cancellationToken);
}
