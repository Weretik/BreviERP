using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition;

public enum SewingCompositionMaterialKind
{
    Fabric = 1,
    Accessory = 2,

    /// <summary>A service priced by the Reference other-cost revision of <c>CostCode</c>.</summary>
    OtherCost = 3
}
