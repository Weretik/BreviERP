using Catalog.Domain.Pricing.Ppe;

namespace Catalog.Application.Features.Pricing.Queries.PreviewPpePrices;

/// <summary>Answer of <c>previewPpePrices</c>: the three prices, or the blockers that stop them.</summary>
public sealed record PpePricePreview(PpePriceTiers? Prices, IReadOnlyList<PpePricingBlocker> Blockers, bool IsCalculated)
{
    public static PpePricePreview From(PpePriceCalculation calculation) =>
        new(calculation.Prices, calculation.Blockers, calculation.IsCalculated);
}
