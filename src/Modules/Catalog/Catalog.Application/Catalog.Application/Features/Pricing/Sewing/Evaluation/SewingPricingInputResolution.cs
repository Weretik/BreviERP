using Catalog.Application.Contracts.Pricing;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.Evaluation;

public static class SewingPricingInputResolution
{
    public static IReadOnlyList<string> Keys(SewingArticlePricingSnapshot snapshot) =>
        snapshot.Materials.Select(x => x.ReferenceKey)
            .Concat(snapshot.Operations.Select(x => $"Operation:{x.OperationId}")).Distinct().ToArray();

    public static IReadOnlyList<SewingMaterialCostInput> Materials(SewingArticlePricingSnapshot snapshot,
        IReadOnlyDictionary<string, PricingReferenceValue> references) =>
        snapshot.Materials.Select(line =>
        {
            references.TryGetValue(line.ReferenceKey, out var value);
            return new SewingMaterialCostInput($"Material:{line.LineId}", line.Quantity, value?.Value,
                line.IsOtherCost, line.IsFabric, line.IsRequired, line.IsReviewed,
                value?.IsActive == true, value?.IsApproved == true, value?.Unit == line.Unit);
        }).ToArray();

    public static IReadOnlyList<SewingOperationCostInput> Operations(SewingArticlePricingSnapshot snapshot,
        IReadOnlyDictionary<string, PricingReferenceValue> references) =>
        snapshot.Operations.Select(line =>
        {
            references.TryGetValue($"Operation:{line.OperationId}", out var value);
            return new SewingOperationCostInput($"Operation:{line.LineId}", line.Quantity, value?.Value,
                line.ManualMinutes, line.IsReviewed, value?.IsActive == true,
                value?.IsApproved == true && value.Unit == "Minutes");
        }).ToArray();
}

