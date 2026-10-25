using Catalog.Domain.Pricing.Ppe;

namespace CatalogImport.PpePricing.Verification;

public sealed record PpePricingDiagnostic(string SaleSku, int? ProductId, Guid? OfferVersionId,
    Guid? RuleVersionId, string FormulaVersion, PpePriceTiers? ArithmeticPrices,
    IReadOnlyList<PpePricingBlocker> StrictBlockers, IReadOnlyList<PpePricingParityValue> Comparisons)
{
    public bool CanPublish => false;
}
