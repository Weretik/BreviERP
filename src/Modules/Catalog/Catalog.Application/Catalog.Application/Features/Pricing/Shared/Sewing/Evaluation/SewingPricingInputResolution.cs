using Catalog.Domain.Pricing.Sewing;
using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Shared.Sewing.Evaluation;

public static class SewingPricingInputResolution
{
    public static IReadOnlyList<string> Keys(SewingArticlePricingSnapshot snapshot) =>
        snapshot.Materials.Select(x => x.ReferenceKey)
            .Concat(snapshot.Operations.Select(x => $"Operation:{x.OperationId}")).Distinct().ToArray();

    public static IReadOnlyList<SewingMaterialCostInput> Materials(SewingArticlePricingSnapshot snapshot,
        IReadOnlyDictionary<string, PricingReferenceValue> references) =>
        Materials(snapshot.Materials, references);

    public static IReadOnlyList<SewingMaterialCostInput> Materials(IReadOnlyList<SewingPricingMaterialRow> materials,
        IReadOnlyDictionary<string, PricingReferenceValue> references) =>
        materials.Select(line =>
        {
            references.TryGetValue(line.ReferenceKey, out var value);
            return new SewingMaterialCostInput($"Material:{line.LineId}", line.Quantity, value?.Value,
                line.IsOtherCost, line.IsFabric,
                value?.IsActive == true, value?.IsApproved == true, value?.Unit == line.Unit);
        }).ToArray();

    public static IReadOnlyList<SewingOperationCostInput> Operations(SewingArticlePricingSnapshot snapshot,
        IReadOnlyDictionary<string, PricingReferenceValue> references) =>
        Operations(snapshot.Operations, references);

    public static IReadOnlyList<SewingOperationCostInput> Operations(IReadOnlyList<SewingPricingOperationRow> operations,
        IReadOnlyDictionary<string, PricingReferenceValue> references) =>
        operations.Select(line =>
        {
            references.TryGetValue($"Operation:{line.OperationId}", out var value);
            return new SewingOperationCostInput($"Operation:{line.LineId}", line.Quantity, value?.Value,
                line.ManualMinutes, value?.IsActive == true,
                value?.IsApproved == true && value.Unit == "Minutes");
        }).ToArray();
}

