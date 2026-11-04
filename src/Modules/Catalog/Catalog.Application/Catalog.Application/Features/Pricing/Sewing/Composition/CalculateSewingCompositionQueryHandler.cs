using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.Composition;

public sealed class CalculateSewingCompositionQueryHandler(
    ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
    : IQueryHandler<CalculateSewingCompositionQuery, Result<SewingCompositionPrice>>
{
    public async ValueTask<Result<SewingCompositionPrice>> Handle(
        CalculateSewingCompositionQuery query, CancellationToken cancellationToken)
    {
        var at = query.At.ToUniversalTime();
        var keys = query.Materials.Select(Key).Concat(query.Operations.Select(x => $"Operation:{x.OperationId}"))
            .Distinct().ToArray();
        var context = await SewingPricingContext.ReadAsync(settings, references, keys, at, cancellationToken);
        if (context.Blockers.Count != 0)
            return Result.Success(new SewingCompositionPrice(at, new(null, null, context.Blockers), context.Values));

        var byKey = context.ByKey;
        var materials = query.Materials.Select((line, index) =>
        {
            var key = Key(line);
            byKey.TryGetValue(key, out var value);
            return new SewingPricingMaterialRow(index + 1, key, line.Quantity, value?.Unit ?? "",
                line.Kind == SewingCompositionMaterialKind.Fabric, false);
        }).ToArray();
        var operations = query.Operations.Select((line, index) =>
            new SewingPricingOperationRow(index + 1, line.OperationId, line.Quantity, line.ManualMinutes)).ToArray();
        var calculation = SewingArticleCalculator.Calculate(context.Candidates[0].Settings,
            SewingPricingInputResolution.Materials(materials, byKey),
            SewingPricingInputResolution.Operations(operations, byKey));
        return Result.Success(new SewingCompositionPrice(at, calculation, context.Values));
    }

    private static string Key(SewingCompositionMaterial line) =>
        (line.Kind == SewingCompositionMaterialKind.Fabric ? "Fabric:" : "Accessory:") + line.ReferenceId;
}
