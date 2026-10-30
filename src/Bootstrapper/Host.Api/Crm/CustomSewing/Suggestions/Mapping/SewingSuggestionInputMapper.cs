using Catalog.Application.Contracts.Pricing;
using Catalog.Application.Contracts.Suggestions;
using Crm.Application.CustomSewing.GetSuggestions.Contracts;

namespace Host.Api.Crm.CustomSewing.Suggestions.Mapping;

internal static class SewingSuggestionInputMapper
{
    internal static IEnumerable<string> ReferenceKeys(SewingArticlePricingSnapshot composition)
        => composition.Materials.Where(x => !x.IsOtherCost).Select(x => x.ReferenceKey)
            .Concat(composition.Operations.Select(x => $"Operation:{x.OperationId}"));

    internal static SuggestionInput Map(SewingSuggestionIdentity identity, string productType, string season,
        SewingArticlePricingSnapshot? composition, ILookup<string, PricingReferenceValue> references)
    {
        var selections = new List<SuggestionSelection>();
        var valid = composition is { IsActive: true }
            && composition.Materials.Where(x => !x.IsOtherCost).All(x => x.IsReviewed)
            && composition.Operations.All(x => x.IsReviewed);
        if (valid)
        {
            foreach (var material in composition!.Materials.Where(x => !x.IsOtherCost))
            {
                var parts = material.ReferenceKey.Split(':');
                var expectedKind = material.IsFabric ? "Fabric" : "Accessory";
                if (parts.Length != 2 || parts[0] != expectedKind || !int.TryParse(parts[1], out var id) || id <= 0
                    || material.ReferenceKey != $"{expectedKind}:{id}")
                {
                    valid = false;
                    break;
                }
                if (!material.IsFabric) selections.Add(new("accessory", id, material.Unit, material.Quantity, null));
            }
            foreach (var operation in composition!.Operations)
                selections.Add(new("operation", operation.OperationId, "Operations", operation.Quantity, operation.ManualMinutes));
            valid = valid && ReferenceKeys(composition).Distinct(StringComparer.Ordinal).All(key =>
                references[key].Count() == 1 && references[key].All(x => x.IsActive && x.IsApproved && x.VersionId > 0));
            valid = valid && composition.Materials.Where(x => !x.IsOtherCost)
                .All(row => references[row.ReferenceKey].Any(x => x.Unit == row.Unit));
            valid = valid && composition.Operations.All(row => references[$"Operation:{row.OperationId}"].All(x => x.Unit == "Minutes"));
        }
        var combined = selections.GroupBy(x => new { x.Kind, x.ReferenceId, x.Unit, x.ManualMinutes })
            .Select(group => new SuggestionSelection(group.Key.Kind, group.Key.ReferenceId, group.Key.Unit,
                group.Sum(x => x.Quantity), group.Key.ManualMinutes)).ToArray();
        return new(identity.ModelCode, productType, season, valid, combined);
    }
}
