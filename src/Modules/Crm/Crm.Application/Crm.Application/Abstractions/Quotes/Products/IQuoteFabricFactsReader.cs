namespace Crm.Application.Abstractions.Quotes.Products;

/// <summary>
/// Current facts of Reference fabrics for the manager line under a КП fabric: composition, density, price per metre and
/// supplier. Implemented in the host over the Reference queries; Crm never reads their tables.
/// </summary>
public interface IQuoteFabricFactsReader
{
    /// <summary>Fabrics that no longer exist are missing from the answer.</summary>
    Task<IReadOnlyList<QuoteFabricFacts>> GetAsync(IReadOnlyCollection<int> fabricIds,
        CancellationToken cancellationToken);
}

public sealed record QuoteFabricFacts(
    int FabricId,
    string Name,
    string? Composition,
    string? Density,
    decimal PricePerMeter,
    int? SupplierId,
    string? SupplierName);
