using Catalog.Contracts.Products;

namespace Catalog.Contracts.Sewing;

public enum SewingPriceMaterialKind
{
    Fabric = 1,
    Accessory = 2,
    OtherCost = 3
}

public sealed record SewingPriceMaterial(SewingPriceMaterialKind Kind, int ReferenceId, decimal Quantity,
    string? CostCode = null);

public sealed record SewingPriceOperation(int OperationId, decimal Quantity, decimal? ManualMinutes = null);

/// <summary>Why a price could not be calculated; <paramref name="InputKey"/> names the line or reference.</summary>
public sealed record SewingPriceBlocker(string Code, string? InputKey);

/// <summary>A reference value the calculation used, such as `OtherCost:{code}`.</summary>
public sealed record SewingPriceReference(string ReferenceKey, decimal? Value, bool IsActive);

/// <summary>
/// The price of a free composition by the Catalog sewing formula. <paramref name="Errors"/> lists input errors
/// (then nothing else is set); <paramref name="Prices"/> is null while <paramref name="Blockers"/> remain.
/// </summary>
public sealed record SewingPriceResult(
    IReadOnlyList<string> Errors,
    CatalogPrices? Prices,
    decimal? TotalMinutes,
    decimal? MinuteCost,
    decimal? FullCost,
    IReadOnlyList<SewingPriceBlocker> Blockers,
    IReadOnlyList<SewingPriceReference> References)
{
    public bool IsCalculated => Errors.Count == 0 && Prices is not null && Blockers.Count == 0;
}


/// <summary>Prices a composition that is not a Catalog model, such as «Виріб з нуля» in a КП.</summary>
public interface ISewingPriceCalculator
{
    Task<SewingPriceResult> CalculateAsync(IReadOnlyList<SewingPriceMaterial> materials,
        IReadOnlyList<SewingPriceOperation> operations, DateTimeOffset at, CancellationToken cancellationToken);
}
