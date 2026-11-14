using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition.Results;

/// <summary>
/// A fabric (metres), an accessory or a service of a composition that is not a Catalog article. A service has no
/// reference row: <paramref name="ReferenceId"/> is 0 and <paramref name="CostCode"/> names it.
/// </summary>
public sealed record SewingCompositionMaterial(
    SewingCompositionMaterialKind Kind, int ReferenceId, decimal Quantity, string? CostCode = null);
