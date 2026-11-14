using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductSection.Import.DTOs;
using Reference.Application.Features.ProductSection.Shared;
using Reference.Application.Features.Shared.SheetImport;
using Reference.Domain.ProductSections.ValueObjects;
using ProductSectionEntity = Reference.Domain.ProductSections.Entities.ProductSection;

namespace Reference.Application.Features.ProductSection.Import;

/// <summary>
/// Imports the Calculator sheet «Типи і розділи»: sections are matched by name (case and punctuation ignored), the
/// row number becomes the display order. A preview changes nothing; an error in any row stops the whole import.
/// With <c>DeleteMissing</c>, Brevi sections that are not in the sheet are deleted. Repeating the same file changes
/// nothing.
/// </summary>
public sealed class ImportProductSectionsCommandHandler(IReferenceRepository<ProductSectionEntity> repository)
    : ICommandHandler<ImportProductSectionsCommand, Result<ProductSectionImportResult>>
{
    public async ValueTask<Result<ProductSectionImportResult>> Handle(
        ImportProductSectionsCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.ListAsync(cancellationToken);
        var byName = existing
            .GroupBy(x => ReferenceNameKey.From(x.Name))
            .ToDictionary(group => group.Key, group => group.OrderBy(x => x.Id.Value).First());
        var nextId = existing.Count == 0 ? 1 : existing.Max(x => x.Id.Value) + 1;

        var rows = new List<ReferenceImportRowResult>();
        var created = new List<ProductSectionEntity>();
        var matched = new HashSet<ProductSectionEntity>();
        var updates = new List<Action>();

        foreach (var row in command.Rows)
        {
            var name = row.Name.Trim();
            var details = new ProductSectionDetails(ProductSectionTypes.Parse(row.Type), name, row.IsActive, row.Comment,
                row.RowNumber);
            var target = byName.GetValueOrDefault(ReferenceNameKey.From(name));

            var checkedRow = ProductSectionEntity.Create(ProductSectionId.From(target?.Id.Value ?? nextId), details);
            if (checkedRow.IsFailure)
            {
                rows.Add(new(row.RowNumber, row.Type, name, ReferenceImportActions.Error, [], checkedRow.Error!.Message));
                continue;
            }

            var candidate = checkedRow.Value;

            if (target is null)
            {
                nextId++;
                created.Add(candidate);
                rows.Add(new(row.RowNumber, row.Type, name, ReferenceImportActions.Create, [], null));
                continue;
            }

            matched.Add(target);
            var changes = new ReferenceImportDiff()
                .Compare("Тип товару", target.Type, candidate.Type)
                .Compare("Назва", target.Name, candidate.Name)
                .Compare("Активний", target.IsActive, candidate.IsActive)
                .Text("Коментар", target.Comment, candidate.Comment)
                .Compare("Порядок", target.SortOrder, candidate.SortOrder)
                .Changes.ToList();
            if (changes.Count > 0)
                updates.Add(() => target.ApplyDetails(details).ThrowIfFailure());
            rows.Add(new(row.RowNumber, row.Type, name,
                changes.Count == 0 ? ReferenceImportActions.Unchanged : ReferenceImportActions.Update, changes, null));
        }

        var deleted = command.DeleteMissing ? existing.Where(x => !matched.Contains(x)).ToList() : [];
        var errors = rows.Count(x => x.Action == ReferenceImportActions.Error);
        var applied = !command.DryRun && errors == 0;

        if (applied)
        {
            foreach (var update in updates)
                update();
            if (created.Count > 0)
                await repository.AddRangeAsync(created, cancellationToken);
            if (deleted.Count > 0)
                await repository.DeleteRangeAsync(deleted, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(new ProductSectionImportResult(
            applied,
            rows.Count(x => x.Action == ReferenceImportActions.Create),
            rows.Count(x => x.Action == ReferenceImportActions.Update),
            rows.Count(x => x.Action == ReferenceImportActions.Unchanged),
            errors,
            deleted.Select(x => x.Name).ToList(),
            rows));
    }
}
