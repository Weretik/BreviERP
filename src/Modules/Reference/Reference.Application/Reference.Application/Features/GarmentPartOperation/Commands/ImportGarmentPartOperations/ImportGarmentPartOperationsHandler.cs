using Reference.Domain.GarmentPartOperations;
using GarmentPartEntity = Reference.Domain.GarmentParts.GarmentPart;
using GarmentPartOperationEntity = Reference.Domain.GarmentPartOperations.GarmentPartOperation;
using Reference.Application.Abstractions;
using Reference.Application.Features.SheetImport.Shared;
using Reference.Domain.GarmentParts;
using Reference.Application.Abstractions.Pricing;

namespace Reference.Application.Features.GarmentPartOperation.Commands.ImportGarmentPartOperations;

/// <summary>
/// Imports the Calculator sheet «Операції». A row is matched by code (OP-…); anything else is created with the
/// next ID, and an unknown «Група деталей» becomes a new group. With <c>DeleteMissing</c>, operations and groups
/// that are not in the sheet are deleted (an operation still in use is hidden instead). A preview changes nothing;
/// a row with an error stops the whole import. Repeating the same file changes nothing.
/// </summary>
public sealed class ImportGarmentPartOperationsHandler(
    IReferenceRepository<GarmentPartOperationEntity> repository,
    IReferenceRepository<GarmentPartEntity> groupRepository,
    IProductUsageReader usageReader)
    : ICommandHandler<ImportGarmentPartOperationsCommand, Result<GarmentPartOperationImportResult>>
{
    public async ValueTask<Result<GarmentPartOperationImportResult>> Handle(
        ImportGarmentPartOperationsCommand command,
        CancellationToken cancellationToken)
    {
        var operations = await repository.ListAsync(cancellationToken);
        var groups = await groupRepository.ListAsync(cancellationToken);
        var groupsByKey = groups.GroupBy(x => ReferenceNameKey.From(x.Name))
            .ToDictionary(group => group.Key, group => group.OrderBy(x => x.Id.Value).First());
        var newGroups = new List<GarmentPartEntity>();
        var nextGroupId = groups.Count == 0 ? 1 : groups.Max(x => x.Id.Value) + 1;
        var byCode = operations.Where(x => x.Code is not null).ToDictionary(x => x.Code!);
        var nextId = operations.Count == 0 ? 1 : operations.Max(x => x.Id.Value) + 1;

        var plans = new List<(GarmentPartOperationEntity? Target, GarmentPartOperationDetails Details)>();
        var results = new List<ReferenceImportRowResult>();
        foreach (var row in command.Rows)
        {
            var code = GarmentPartOperationEntity.NormalizeCode(row.Code)!;
            var target = byCode.GetValueOrDefault(code);
            var groupError = string.IsNullOrWhiteSpace(row.GroupName) ? "Немає групи деталей." : null;
            var group = groupError is null ? ResolveGroup(row.GroupName) : null;
            var details = new GarmentPartOperationDetails(code, group?.Id ?? GarmentPartId.From(1), row.Element,
                row.Variant, row.Name, row.Min, row.MinutesPerProduct, row.IsActive, row.Notes);
            var error = groupError ?? Validate(details);
            if (error is not null)
            {
                results.Add(new(row.RowNumber, code, row.Name, ReferenceImportActions.Error, [], error));
                continue;
            }

            var changes = target is null ? [] : Diff(target, details, groups.Concat(newGroups));
            var action = target is null ? ReferenceImportActions.Create
                : changes.Count == 0 ? ReferenceImportActions.Unchanged : ReferenceImportActions.Update;
            results.Add(new(row.RowNumber, code, row.Name.Trim(), action, changes, null));
            plans.Add((target, details));
        }

        var matched = plans.Where(x => x.Target is not null).Select(x => x.Target!).ToHashSet();
        var deleted = new List<GarmentPartOperationEntity>();
        var keptInUse = new List<GarmentPartOperationEntity>();
        if (command.DeleteMissing)
        {
            foreach (var operation in operations.Where(x => !matched.Contains(x)))
            {
                if (await usageReader.IsGarmentPartOperationUsedAsync(operation.Id.Value, cancellationToken))
                    keptInUse.Add(operation);
                else
                    deleted.Add(operation);
            }
        }

        var usedGroupIds = plans.Select(x => x.Details.GarmentPartId)
            .Concat(keptInUse.Select(x => x.GarmentPartId))
            .Concat(operations.Where(x => !command.DeleteMissing && !matched.Contains(x)).Select(x => x.GarmentPartId))
            .ToHashSet();
        var deletedGroups = command.DeleteMissing
            ? groups.Where(x => !usedGroupIds.Contains(x.Id)).ToList()
            : [];

        var hasErrors = results.Any(x => x.Action == ReferenceImportActions.Error);
        var applied = !command.DryRun && !hasErrors;
        if (applied)
        {
            try
            {
                await Apply(plans, keptInUse, deleted, newGroups, deletedGroups, nextId, cancellationToken);
            }
            catch (ReferencePricingConflictException exception)
            {
                return Result.Conflict(exception.Message);
            }
        }

        return Result.Success(new GarmentPartOperationImportResult(
            applied,
            results.Count(x => x.Action == ReferenceImportActions.Create),
            results.Count(x => x.Action == ReferenceImportActions.Update),
            results.Count(x => x.Action == ReferenceImportActions.Unchanged),
            results.Count(x => x.Action == ReferenceImportActions.Error),
            newGroups.Select(x => x.Name).ToList(),
            deleted.Select(Label).ToList(),
            keptInUse.Select(Label).ToList(),
            deletedGroups.Select(x => x.Name).ToList(),
            results));

        GarmentPartEntity ResolveGroup(string name)
        {
            var key = ReferenceNameKey.From(name);
            if (groupsByKey.TryGetValue(key, out var existing))
                return existing;
            // The caller skips a blank group name, the only rule of a group.
            var created = GarmentPartEntity.Create(GarmentPartId.From(nextGroupId++), name.Trim()).Value;
            newGroups.Add(created);
            groupsByKey[key] = created;
            return created;
        }
    }

    private async Task Apply(
        List<(GarmentPartOperationEntity? Target, GarmentPartOperationDetails Details)> plans,
        List<GarmentPartOperationEntity> keptInUse,
        List<GarmentPartOperationEntity> deleted,
        List<GarmentPartEntity> newGroups,
        List<GarmentPartEntity> deletedGroups,
        int nextId,
        CancellationToken cancellationToken)
    {
        // Deletes go first so that a new row never collides with an old one on the (group, name) index.
        if (deleted.Count > 0)
            await repository.DeleteRangeAsync(deleted, cancellationToken);
        if (newGroups.Count > 0)
            await groupRepository.AddRangeAsync(newGroups, cancellationToken);

        var created = new List<GarmentPartOperationEntity>();
        foreach (var (target, details) in plans)
        {
            // Every planned row passed Validate, so these changes cannot fail.
            if (target is null)
                created.Add(GarmentPartOperationEntity.Create(GarmentPartOperationId.From(nextId++), details).Value);
            else
                target.ApplyDetails(details).ThrowIfFailure();
        }

        foreach (var operation in keptInUse)
            operation.ApplyDetails(DetailsOf(operation) with { IsActive = false }).ThrowIfFailure();

        if (created.Count > 0)
            await repository.AddRangeAsync(created, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        if (deletedGroups.Count > 0)
            await groupRepository.DeleteRangeAsync(deletedGroups, cancellationToken);
    }

    private static string? Validate(GarmentPartOperationDetails details)
    {
        if (string.IsNullOrWhiteSpace(details.Name))
            return "Немає назви операції.";
        if (!details.MinutesPerProduct && details.Min <= 0)
            return "Немає норми часу.";
        return GarmentPartOperationEntity.Create(GarmentPartOperationId.From(1), details).Error?.Message;
    }

    private static List<string> Diff(
        GarmentPartOperationEntity operation,
        GarmentPartOperationDetails details,
        IEnumerable<GarmentPartEntity> groups)
    {
        var names = groups.ToDictionary(x => x.Id, x => x.Name);
        return new ReferenceImportDiff()
            .Text("Група", names.GetValueOrDefault(operation.GarmentPartId), names.GetValueOrDefault(details.GarmentPartId))
            .Text("Вузол / елемент", operation.Element, details.Element)
            .Text("Варіант", operation.Variant, details.Variant)
            .Text("Назва", operation.Name, details.Name)
            .Compare("Норма часу", operation.Min, details.MinutesPerProduct ? 0m : details.Min)
            .Compare("Час для кожного виробу", operation.MinutesPerProduct, details.MinutesPerProduct)
            .Compare("Активна", operation.IsActive, details.IsActive)
            .Text("Примітки", operation.Notes, details.Notes)
            .Changes.ToList();
    }

    private static GarmentPartOperationDetails DetailsOf(GarmentPartOperationEntity operation) => new(
        operation.Code, operation.GarmentPartId, operation.Element, operation.Variant, operation.Name, operation.Min,
        operation.MinutesPerProduct, operation.IsActive, operation.Notes);

    private static string Label(GarmentPartOperationEntity operation) =>
        operation.Code is null ? $"{operation.Id.Value} {operation.Name}" : $"{operation.Code} {operation.Name}";
}
