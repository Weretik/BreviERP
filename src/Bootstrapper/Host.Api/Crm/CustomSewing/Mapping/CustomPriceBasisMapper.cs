using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;
using Crm.Domain.CustomSewing.Composition;
using Crm.Domain.CustomSewing.Pricing;

namespace Host.Api.Crm.CustomSewing.Mapping;

internal static class CustomPriceBasisMapper
{
    internal static CustomPriceBasis Map(SewingCustomPricePreview value, CustomConfiguration input, DateTimeOffset at,
        bool artworkVerified = false)
    {
        var calculation = value.Calculation;
        var blockers = calculation.Blockers.Select(x => x.Code).Concat(CustomCalculationReadiness.Inspect(input,
            calculation.Prices?.Retail, calculation.Prices?.Wholesale, calculation.Prices?.LargeWholesale, artworkVerified)).Distinct().ToArray();
        var breakdown = calculation.Breakdown;
        return new CustomPriceBasis(Guid.NewGuid(), at.ToUniversalTime(), at.ToUniversalTime().AddMinutes(15),
            SewingCustomCalculation.FormulaVersion, value.Settings?.Id,
            value.ReferenceVersions.Select(x => new CustomReferenceBasis(x.ReferenceKey, x.VersionId, x.Value, x.Unit, x.SourceCode)).ToArray(),
            breakdown is null ? null : new CustomCostBreakdown(breakdown.Materials, breakdown.OtherCosts, breakdown.TotalMinutes,
                breakdown.MinuteCost, breakdown.Seamstress, breakdown.Cutting, breakdown.Master, breakdown.Manager,
                breakdown.Bonus, breakdown.Overhead, input.Delivery, input.Packaging,
                input.Applications.Sum(x => x.Cost), input.Applications.Sum(x => x.SaleAmount)),
            blockers.Length == 0 ? calculation.InternalCost : null, blockers.Length == 0 ? calculation.Prices?.Retail : null,
            blockers.Length == 0 ? calculation.Prices?.Wholesale : null, blockers.Length == 0 ? calculation.Prices?.LargeWholesale : null,
            blockers);
    }
}
