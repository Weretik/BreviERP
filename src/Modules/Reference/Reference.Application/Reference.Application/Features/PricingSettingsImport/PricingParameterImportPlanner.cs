using Reference.Application.Features.PricingSettingsImport.DTOs;
using Reference.Application.Features.Shared.SheetImport;
using Reference.Domain.PricingParameters;
using PricingParameterEntity = Reference.Domain.PricingParameters.PricingParameter;

namespace Reference.Application.Features.PricingSettingsImport;

/// <summary>Plans the import of the parameters table: match by code, create the rest with the next ID.</summary>
internal static class PricingParameterImportPlanner
{
    public static ImportPlan<PricingParameterEntity> Plan(
        IReadOnlyList<PricingParameterImportRow> rows, List<PricingParameterEntity> existing, bool deleteMissing)
    {
        var plan = new ImportPlan<PricingParameterEntity>();
        var byCode = existing.ToDictionary(x => x.Code, StringComparer.Ordinal);
        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;

        foreach (var row in rows)
        {
            var code = PricingParameterEntity.NormalizeCode(row.Code)!;
            var details = new PricingParameterDetails(row.Group, row.Name, row.IsCalculated ? null : row.Value,
                row.Unit, row.IsCalculated, row.Comment, row.RowNumber);
            var target = byCode.GetValueOrDefault(code);
            var label = row.Name?.Trim() ?? string.Empty;

            var checkedRow = PricingParameterEntity.Create(PricingParameterId.From(target?.Id.Value ?? nextId), code,
                details, row.UpdatedOn);
            if (checkedRow.IsFailure)
            {
                plan.Error(row.RowNumber, code, label, checkedRow.Error!.Message);
                continue;
            }

            var candidate = checkedRow.Value;

            if (target is null)
            {
                nextId++;
                plan.Create(row.RowNumber, code, label, candidate);
                continue;
            }

            var changes = new ReferenceImportDiff()
                .Text("Група", target.Group, candidate.Group)
                .Text("Назва", target.Name, candidate.Name)
                .Compare("Значення", target.Value, candidate.Value)
                .Text("Одиниця", target.Unit, candidate.Unit)
                .Compare("Тип", target.IsCalculated, candidate.IsCalculated)
                .Compare("Дата оновлення", target.UpdatedOn, candidate.UpdatedOn)
                .Text("Коментар", target.Comment, candidate.Comment)
                .Compare("Порядок", target.SortOrder, candidate.SortOrder)
                .Changes.ToList();
            plan.Match(row.RowNumber, code, label, target, changes,
                () => target.ApplyDetails(details, row.UpdatedOn).ThrowIfFailure());
        }

        plan.Finish(existing, deleteMissing, x => $"{x.Code} {x.Name}");
        return plan;
    }
}
