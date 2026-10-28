using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Features.Pricing.Sewing.Evaluation;
using Catalog.Domain.Pricing.Sewing;

namespace Catalog.Application.Features.Pricing.Sewing.CustomEvaluation;

public sealed class SewingCustomPricingEvaluator(
    ISewingPricingSettingsReader settings, IPricingReferenceVersionReader references)
{
    public async ValueTask<Result<SewingCustomPricePreview>> EvaluateAsync(
        SewingCustomPricingInput input, DateTimeOffset at, CancellationToken cancellationToken)
    {
        if (input is null || !SewingCustomInputValidation.IsValid(input))
            return Result.Invalid(new ValidationError("composition", "invalid_custom_composition"));
        cancellationToken.ThrowIfCancellationRequested();
        input = input with { Materials = input.Materials.ToArray(), Operations = input.Operations.ToArray() };
        at = at.ToUniversalTime();
        var candidates = (await settings.ReadCandidatesAsync(at, cancellationToken))
            .Where(x => x.EffectiveFrom <= at).OrderByDescending(x => x.EffectiveFrom)
            .ThenByDescending(x => x.Id).ToArray();
        var selected = candidates.FirstOrDefault();
        var blockers = new List<SewingPricingBlocker>();
        if (selected is null)
            blockers.Add(new("MissingSettings", null, "No approved effective sewing settings exist."));
        else
        {
            if (candidates.Length > 1 && candidates[1].EffectiveFrom == selected.EffectiveFrom)
                blockers.Add(new("AmbiguousSettings", null, "Multiple settings versions have the same effective instant."));
            if (selected.Id <= 0 || selected.Revision <= 0)
                blockers.Add(new("InvalidSettingsVersion", null, "The settings version identity is invalid."));
            if (selected.FormulaVersion != SewingArticleCalculation.FormulaVersion)
                blockers.Add(new("UnsupportedFormula", null, "Settings require a different formula implementation."));
        }
        var keys = input.Materials.Select(x => x.ReferenceKey)
            .Concat(input.Operations.Select(x => $"Operation:{x.OperationId}")).Distinct().ToArray();
        var values = (await references.ReadAsync(keys, at, cancellationToken))
            .Where(x => keys.Contains(x.ReferenceKey, StringComparer.Ordinal)).ToArray();
        foreach (var group in values.GroupBy(x => x.ReferenceKey).Where(x => x.Count() > 1))
            blockers.Add(new("AmbiguousReferenceVersion", group.Key, "Multiple effective reference versions exist."));
        foreach (var value in values.Where(x => x.VersionId <= 0))
            blockers.Add(new("InvalidReferenceVersion", value.ReferenceKey, "Reference version identity is invalid."));
        if (blockers.Count != 0)
            return Result.Success(new SewingCustomPricePreview(at, input, selected, values,
                new(null, input.Charges, null, null, blockers)));
        var byKey = values.ToDictionary(x => x.ReferenceKey);
        var calculation = SewingCustomCalculator.Calculate(selected!.Settings,
            SewingPricingInputResolution.Materials(input.Materials, byKey),
            SewingPricingInputResolution.Operations(input.Operations, byKey), input.Charges);
        return Result.Success(new SewingCustomPricePreview(at, input, selected, values, calculation));
    }
}
