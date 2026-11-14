using Catalog.Application.Features.Pricing.Shared.Ppe.SupplierPrices.Sources;

namespace Catalog.Application.Abstractions.Pricing.PpeSupplierPrices.Sources;

/// <summary>Reads supplier price files; a file that cannot be read returns an invalid result with the reason.</summary>
public interface IPpeSupplierFileReader
{
    Task<Result<IReadOnlyList<SupplierFilePrice>>> ReadAsync(
        PpePriceFileKind kind, IReadOnlyList<SupplierPriceFile> files, CancellationToken cancellationToken);
}
