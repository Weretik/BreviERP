using Reference.Application.Features.PricingSettingsImport.DTOs;
using Reference.Application.Features.Shared.SheetImport;
using Reference.Domain.PpeMarkupRules.ValueObjects;
using PpeMarkupRuleEntity = Reference.Domain.PpeMarkupRules.Entities.PpeMarkupRule;

namespace Reference.Application.Features.PricingSettingsImport;

/// <summary>Plans the import of the PPE markup rules table: match by name, create the rest with the next ID.</summary>
internal static class PpeMarkupRuleImportPlanner
{
    public static ImportPlan<PpeMarkupRuleEntity> Plan(
        IReadOnlyList<PpeMarkupRuleImportRow> rows, List<PpeMarkupRuleEntity> existing, bool deleteMissing)
    {
        var plan = new ImportPlan<PpeMarkupRuleEntity>();
        var byName = existing.ToDictionary(x => x.Name, StringComparer.Ordinal);
        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;

        foreach (var row in rows)
        {
            var name = row.Name.Trim();
            var details = new PpeMarkupRuleDetails(row.RetailMarkup, row.WholesaleMarkup, row.LargeWholesaleMarkup,
                row.PriceNote, row.AppliesTo, row.RowNumber);
            var target = byName.GetValueOrDefault(name);

            var checkedRow = PpeMarkupRuleEntity.Create(PpeMarkupRuleId.From(target?.Id.Value ?? nextId), name, details);
            if (checkedRow.IsFailure)
            {
                plan.Error(row.RowNumber, name, name, checkedRow.Error!.Message);
                continue;
            }

            var candidate = checkedRow.Value;

            if (target is null)
            {
                nextId++;
                plan.Create(row.RowNumber, name, name, candidate);
                continue;
            }

            var changes = new ReferenceImportDiff()
                .Compare("Роздріб", target.RetailMarkup, candidate.RetailMarkup)
                .Compare("Опт", target.WholesaleMarkup, candidate.WholesaleMarkup)
                .Compare("Великий опт", target.LargeWholesaleMarkup, candidate.LargeWholesaleMarkup)
                .Text("Примітка для прайсу", target.PriceNote, candidate.PriceNote)
                .Text("До яких товарів належить", target.AppliesTo, candidate.AppliesTo)
                .Compare("Порядок", target.SortOrder, candidate.SortOrder)
                .Changes.ToList();
            plan.Match(row.RowNumber, name, name, target, changes, () => target.ApplyDetails(details).ThrowIfFailure());
        }

        plan.Finish(existing, deleteMissing, x => x.Name);
        return plan;
    }
}
