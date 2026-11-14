using Reference.Domain.GarmentAccessories;
using GarmentAccessoryEntity = Reference.Domain.GarmentAccessories.GarmentAccessory;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Abstractions;
using Reference.Application.Features.SheetImport.Shared;
using Reference.Application.Abstractions.Pricing;
using BuildingBlocks.Domain;

namespace Reference.Application.Features.GarmentAccessory.Commands.ImportGarmentAccessories;

/// <summary>
/// Imports the Calculator sheet «Фурнітура». A row is matched by code (FUR-…); an older Brevi accessory without
/// a code is linked when its name matches. Anything else is created with the next ID. A preview changes
/// nothing; a row with an error stops the whole import. Repeating the same file changes nothing.
/// </summary>
public sealed class ImportGarmentAccessoriesHandler(
    IReferenceRepository<GarmentAccessoryEntity> repository,
    IReferenceRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<ImportGarmentAccessoriesCommand, Result<ReferenceImportResult>>
{
    public async ValueTask<Result<ReferenceImportResult>> Handle(
        ImportGarmentAccessoriesCommand command,
        CancellationToken cancellationToken)
    {
        var accessories = await repository.ListAsync(cancellationToken);
        var suppliers = new ImportSupplierResolver(await supplierRepository.ListAsync(cancellationToken));
        var byCode = accessories.Where(x => x.Code is not null).ToDictionary(x => x.Code!);
        var uncodedByName = accessories.Where(x => x.Code is null)
            .GroupBy(x => ReferenceNameKey.From(x.Name))
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single());
        var nextId = accessories.Count == 0 ? 1 : accessories.Max(x => x.Id.Value) + 1;

        var plans = new List<(GarmentAccessoryImportRow Row, GarmentAccessoryEntity? Target, GarmentAccessoryDetails? Details)>();
        var results = new List<ReferenceImportRowResult>();
        foreach (var row in command.Rows)
        {
            var code = GarmentAccessoryEntity.NormalizeCode(row.Code)!;
            var target = byCode.GetValueOrDefault(code) ?? Take(uncodedByName, row.Name);
            var unit = string.IsNullOrWhiteSpace(row.Unit)
                ? GarmentAccessoryEntity.DefaultUnit
                : GarmentAccessoryEntity.NormalizeUnit(row.Unit) ?? row.Unit;
            var details = new GarmentAccessoryDetails(code, row.Name, suppliers.Resolve(row.SupplierName).Id, unit,
                row.IsActive, row.Comment);
            var error = Validate(details, row);
            if (error is not null)
            {
                results.Add(new(row.RowNumber, code, row.Name, ReferenceImportActions.Error, [], error));
                plans.Add((row, target, null));
                continue;
            }

            var changes = target is null ? [] : Diff(target, details, row);
            var action = target is null ? ReferenceImportActions.Create
                : changes.Count == 0 ? ReferenceImportActions.Unchanged : ReferenceImportActions.Update;
            results.Add(new(row.RowNumber, code, row.Name.Trim(), action, changes, null));
            plans.Add((row, target, details));
        }

        var matched = plans.Where(x => x.Target is not null).Select(x => x.Target!).ToHashSet();
        var deactivated = command.DeactivateMissing
            ? accessories.Where(x => x.IsActive && !matched.Contains(x)).ToList()
            : [];
        var hasErrors = results.Any(x => x.Action == ReferenceImportActions.Error);
        var applied = !command.DryRun && !hasErrors;

        if (applied)
        {
            if (suppliers.Created.Count > 0)
                await supplierRepository.AddRangeAsync(suppliers.Created, cancellationToken);

            var created = new List<GarmentAccessoryEntity>();
            foreach (var (row, target, details) in plans)
            {
                var price = MoneyAmount.From(row.Price);
                if (target is null)
                {
                    // Every planned row passed Validate, so these changes cannot fail.
                    created.Add(GarmentAccessoryEntity.Create(
                        GarmentAccessoryId.From(nextId++), details!, price, row.PriceUpdatedAt).Value);
                    continue;
                }

                target.ApplyDetails(details!).ThrowIfFailure();
                if (target.Price != price || target.PriceUpdatedAt != row.PriceUpdatedAt)
                    target.SetPrice(price, row.PriceUpdatedAt).ThrowIfFailure();
            }

            foreach (var accessory in deactivated)
                accessory.ApplyDetails(DetailsOf(accessory) with { IsActive = false }).ThrowIfFailure();

            try
            {
                if (created.Count > 0)
                    await repository.AddRangeAsync(created, cancellationToken);
                await repository.SaveChangesAsync(cancellationToken);
            }
            catch (ReferencePricingConflictException exception)
            {
                return Result.Conflict(exception.Message);
            }
        }

        return Result.Success(new ReferenceImportResult(
            applied,
            results.Count(x => x.Action == ReferenceImportActions.Create),
            results.Count(x => x.Action == ReferenceImportActions.Update),
            results.Count(x => x.Action == ReferenceImportActions.Unchanged),
            results.Count(x => x.Action == ReferenceImportActions.Error),
            suppliers.Created.Select(x => x.Name).ToList(),
            deactivated.Select(x => x.Code is null ? x.Name : $"{x.Code} {x.Name}").ToList(),
            results));
    }

    private static GarmentAccessoryEntity? Take(Dictionary<string, GarmentAccessoryEntity> uncodedByName, string name) =>
        uncodedByName.Remove(ReferenceNameKey.From(name), out var accessory) ? accessory : null;

    private static string? Validate(GarmentAccessoryDetails details, GarmentAccessoryImportRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
            return "Немає назви фурнітури.";
        if (row.Price is < 0 or > 10_000)
            return "Ціна має бути від 0 до 10000 грн.";
        if (GarmentAccessoryEntity.NormalizeUnit(details.Unit) is null)
            return $"Невідома одиниця «{details.Unit}». Можна: {string.Join(", ", GarmentAccessoryEntity.Units)}.";
        return GarmentAccessoryEntity.Create(
                GarmentAccessoryId.From(1), details, MoneyAmount.From(row.Price), row.PriceUpdatedAt)
            .Error?.Message;
    }

    private static List<string> Diff(GarmentAccessoryEntity accessory, GarmentAccessoryDetails details, GarmentAccessoryImportRow row) =>
        new ReferenceImportDiff()
            .Text("Код", accessory.Code, details.Code)
            .Text("Назва", accessory.Name, details.Name)
            .Compare("Постачальник", accessory.SupplierId, details.SupplierId)
            .Compare("Ціна", accessory.Price.Value, MoneyAmount.From(row.Price).Value)
            .Compare("Одиниця", accessory.Unit, details.Unit)
            .Compare("Дата ціни", accessory.PriceUpdatedAt, row.PriceUpdatedAt)
            .Compare("Активна", accessory.IsActive, details.IsActive)
            .Text("Коментар", accessory.Comment, details.Comment)
            .Changes.ToList();

    private static GarmentAccessoryDetails DetailsOf(GarmentAccessoryEntity accessory) => new(
        accessory.Code, accessory.Name, accessory.SupplierId, accessory.Unit, accessory.IsActive, accessory.Comment);
}
