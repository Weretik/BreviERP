using Catalog.Domain.Pricing.Sewing;
using Reference.Domain.Pricing;

namespace SewingPricing.Jobs.Bootstrap;

public static class PricingBaselineReconciler
{
    public static void Verify(SewingPricingSettingsVersion settingsVersion, SewingCalculationSettings settings,
        IReadOnlyList<ReferencePricingInputVersion> stored, IReadOnlyList<ReferencePricingInputVersion> expected)
    {
        if (settingsVersion.ToSettings() != settings || !settingsVersion.IsApproved
            || settingsVersion.FormulaVersion != SewingArticleCalculation.FormulaVersion
            || stored.Count != expected.Count || expected.Any(input => !stored.Any(value =>
                value.SourceCode == input.SourceCode && value.Value == input.Value && value.Unit == input.Unit
                && value.FabricId == input.FabricId && value.AccessoryId == input.AccessoryId
                && value.OperationId == input.OperationId && value.OtherCostCode == input.OtherCostCode
                && value.IsActive == input.IsActive && value.IsApproved == input.IsApproved
                && value.EffectiveFrom == input.EffectiveFrom && value.Revision == input.Revision)))
            throw new InvalidDataException("The existing pricing bootstrap does not reconcile with the frozen source.");
    }
}

