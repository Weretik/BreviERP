using Catalog.Contracts.Products;
using Catalog.Contracts.Sewing;
using Mediator;
using Catalog.Application.Features.Pricing.Queries.CalculateSewingComposition;

namespace Catalog.Infrastructure.Contracts.Sewing;

/// <summary>Prices a free composition through the Catalog sewing formula query.</summary>
internal sealed class SewingPriceCalculator(ISender sender) : ISewingPriceCalculator
{
    public async Task<SewingPriceResult> CalculateAsync(IReadOnlyList<SewingPriceMaterial> materials,
        IReadOnlyList<SewingPriceOperation> operations, DateTimeOffset at, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CalculateSewingCompositionQuery(
            materials.Select(x => new SewingCompositionMaterial((SewingCompositionMaterialKind)(int)x.Kind,
                x.ReferenceId, x.Quantity, x.CostCode)).ToArray(),
            operations.Select(x => new SewingCompositionOperation(x.OperationId, x.Quantity, x.ManualMinutes))
                .ToArray(),
            at), cancellationToken);
        if (!result.IsSuccess)
        {
            var errors = result.ValidationErrors.Select(x => x.ErrorMessage).ToArray();
            return new SewingPriceResult(errors.Length > 0 ? errors : ["Склад не пройшов перевірку."], null, null, null,
                null, [], []);
        }

        var calculation = result.Value.Calculation;
        var breakdown = calculation.Breakdown;
        return new SewingPriceResult([],
            calculation.Prices is { } prices
                ? new CatalogPrices(prices.Retail, prices.Wholesale, prices.LargeWholesale)
                : null,
            breakdown?.TotalMinutes, breakdown?.MinuteCost, breakdown?.FullCost,
            calculation.Blockers.Select(x => new SewingPriceBlocker(x.Code, x.InputKey)).ToArray(),
            result.Value.References.Select(x => new SewingPriceReference(x.ReferenceKey, x.Value, x.IsActive)).ToArray());
    }
}
