using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Contracts.Pricing;

public enum SewingCompositionMaterialKind
{
    Fabric = 1,
    Accessory = 2,

    /// <summary>A service priced by the Reference other-cost revision of <c>CostCode</c>.</summary>
    OtherCost = 3
}

/// <summary>
/// A fabric (metres), an accessory or a service of a composition that is not a Catalog article. A service has no
/// reference row: <paramref name="ReferenceId"/> is 0 and <paramref name="CostCode"/> names it.
/// </summary>
public sealed record SewingCompositionMaterial(
    SewingCompositionMaterialKind Kind, int ReferenceId, decimal Quantity, string? CostCode = null);

/// <summary>
/// A sewing operation and how many times it is done; <paramref name="ManualMinutes"/> replaces the norm (needed for
/// an operation without a fixed norm).
/// </summary>
public sealed record SewingCompositionOperation(int OperationId, decimal Quantity, decimal? ManualMinutes = null);

/// <summary>
/// The price of a free composition by the Catalog formula and the effective settings and reference versions.
/// <see cref="References"/> holds the unit cost or minutes of each line, keyed `Fabric:{id}`, `Accessory:{id}`,
/// `OtherCost:{code}` and `Operation:{id}`.
/// </summary>
public sealed record SewingCompositionPrice(
    DateTimeOffset At, SewingArticleCalculation Calculation, IReadOnlyList<PricingReferenceValue> References);
