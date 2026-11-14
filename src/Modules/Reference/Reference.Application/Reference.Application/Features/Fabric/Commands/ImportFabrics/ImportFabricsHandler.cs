using Reference.Domain.GarmentAccessories;
using FabricEntity = Reference.Domain.Fabrics.Fabric;
using SupplierEntity = Reference.Domain.Suppliers.Supplier;
using Reference.Application.Abstractions;
using Reference.Application.Features.SheetImport.Shared;
using Reference.Domain.Fabrics;
using Reference.Application.Abstractions.Pricing;
using BuildingBlocks.Domain;

namespace Reference.Application.Features.Fabric.Commands.ImportFabrics;

/// <summary>
/// Imports the Calculator sheet «Тканини». A row is matched by code (FAB-…); an older Brevi fabric without a
/// code is linked when its name matches. Anything else is created with the next ID. A preview changes nothing;
/// a row with an error stops the whole import. Repeating the same file changes nothing.
/// </summary>
public sealed class ImportFabricsHandler(
    IReferenceRepository<FabricEntity> repository,
    IReferenceRepository<SupplierEntity> supplierRepository)
    : ICommandHandler<ImportFabricsCommand, Result<ReferenceImportResult>>
{
    public async ValueTask<Result<ReferenceImportResult>> Handle(
        ImportFabricsCommand command,
        CancellationToken cancellationToken)
    {
        var fabrics = await repository.ListAsync(cancellationToken);
        var suppliers = new ImportSupplierResolver(await supplierRepository.ListAsync(cancellationToken));
        var byCode = fabrics.Where(x => x.Code is not null).ToDictionary(x => x.Code!);
        var uncodedByName = fabrics.Where(x => x.Code is null)
            .GroupBy(x => ReferenceNameKey.From(x.Name))
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single());
        var nextId = fabrics.Count == 0 ? 1 : fabrics.Max(x => x.Id.Value) + 1;

        var plans = new List<(FabricImportRow Row, FabricEntity? Target, FabricDetails? Details)>();
        var results = new List<ReferenceImportRowResult>();
        foreach (var row in command.Rows)
        {
            var code = FabricEntity.NormalizeCode(row.Code)!;
            var target = byCode.GetValueOrDefault(code) ?? Take(uncodedByName, row.Name);
            var details = new FabricDetails(code, row.Name, row.NameRu, suppliers.Resolve(row.SupplierName).Id,
                row.Composition, row.CompositionRu, row.Density, row.IsActive, row.Comment, row.ProposalDescription);
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
            ? fabrics.Where(x => x.IsActive && !matched.Contains(x)).ToList()
            : [];
        var hasErrors = results.Any(x => x.Action == ReferenceImportActions.Error);
        var applied = !command.DryRun && !hasErrors;

        if (applied)
        {
            if (suppliers.Created.Count > 0)
                await supplierRepository.AddRangeAsync(suppliers.Created, cancellationToken);

            var created = new List<FabricEntity>();
            foreach (var (row, target, details) in plans)
            {
                var price = MoneyAmount.From(row.Price);
                if (target is null)
                {
                    // Every planned row passed Validate, so these changes cannot fail.
                    created.Add(FabricEntity.Create(FabricId.From(nextId++), details!, price, row.PriceUpdatedAt).Value);
                    continue;
                }

                target.ApplyDetails(details!).ThrowIfFailure();
                if (target.Price != price || target.PriceUpdatedAt != row.PriceUpdatedAt)
                    target.SetPrice(price, row.PriceUpdatedAt).ThrowIfFailure();
            }

            foreach (var fabric in deactivated)
                fabric.ApplyDetails(DetailsOf(fabric) with { IsActive = false }).ThrowIfFailure();

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

    private static FabricEntity? Take(Dictionary<string, FabricEntity> uncodedByName, string name) =>
        uncodedByName.Remove(ReferenceNameKey.From(name), out var fabric) ? fabric : null;

    private static string? Validate(FabricDetails details, FabricImportRow row)
    {
        if (string.IsNullOrWhiteSpace(row.Name))
            return "Немає назви тканини.";
        if (row.Price is < 0 or > 10_000)
            return "Ціна має бути від 0 до 10000 грн.";
        return FabricEntity.Create(FabricId.From(1), details, MoneyAmount.From(row.Price), row.PriceUpdatedAt)
            .Error?.Message;
    }

    private static List<string> Diff(FabricEntity fabric, FabricDetails details, FabricImportRow row) =>
        new ReferenceImportDiff()
            .Text("Код", fabric.Code, details.Code)
            .Text("Назва", fabric.Name, details.Name)
            .Text("Назва (RU)", fabric.NameRu, details.NameRu)
            .Compare("Постачальник", fabric.ProviderId, details.ProviderId)
            .Text("Склад", fabric.Composition, details.Composition)
            .Text("Склад (RU)", fabric.CompositionRu, details.CompositionRu)
            .Text("Щільність", fabric.Density, details.Density)
            .Compare("Ціна", fabric.Price.Value, MoneyAmount.From(row.Price).Value)
            .Compare("Дата ціни", fabric.PriceUpdatedAt, row.PriceUpdatedAt)
            .Compare("Активна", fabric.IsActive, details.IsActive)
            .Text("Коментар", fabric.Comment, details.Comment)
            .Text("Опис для КП", fabric.ProposalDescription, details.ProposalDescription)
            .Changes.ToList();

    private static FabricDetails DetailsOf(FabricEntity fabric) => new(
        fabric.Code, fabric.Name, fabric.NameRu, fabric.ProviderId, fabric.Composition, fabric.CompositionRu,
        fabric.Density, fabric.IsActive, fabric.Comment, fabric.ProposalDescription);
}
