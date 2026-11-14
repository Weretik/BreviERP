using BuildingBlocks.Domain.ValueObjects;
using Moq;
using Reference.Domain.GarmentAccessories;
using Reference.Domain.Suppliers;
using Reference.Application.Abstractions;
using Reference.Application.Features.Fabric.Commands.ImportFabrics;
using Reference.Application.Features.GarmentAccessory.Commands.ImportGarmentAccessories;
using Reference.Application.Features.SheetImport.Shared;

namespace UnitTests.Reference;

/// <summary>Import of the Calculator sheets «Тканини» and «Фурнітура» (REF-002).</summary>
public sealed class ReferenceSheetImportTests
{
    private static readonly DateOnly SheetDate = new(2026, 8, 30);

    private static FabricImportRow FabricRow(string code, string name, string? supplier, decimal price, int row = 2) =>
        new(row, code, name, null, supplier, "100% бавовна", null, "260", price, SheetDate, true, null, null);

    private static GarmentAccessoryImportRow AccessoryRow(string code, string name, string? unit, int row = 2) =>
        new(row, code, name, "Веллтекс-Киев", 48m, unit, SheetDate, true, null);

    private static Mock<IReferenceRepository<T>> Repository<T>(List<T> stored, List<T> added) where T : class, BuildingBlocks.Domain.Abstractions.IAggregateRoot
    {
        var repository = new Mock<IReferenceRepository<T>>();
        repository.Setup(x => x.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stored);
        repository.Setup(x => x.AddRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<T>, CancellationToken>((rows, _) => added.AddRange(rows))
            .ReturnsAsync((IEnumerable<T> rows, CancellationToken _) => rows);
        return repository;
    }

    private static List<Supplier> Suppliers() =>
    [
        Supplier.Create(SupplierId.From(1), "Не указан").Value,
        Supplier.Create(SupplierId.From(5), "Спецтекстиль, ТОВ").Value,
    ];

    [Test]
    public async Task Fabric_import_updates_by_code_links_by_name_creates_the_rest_and_new_suppliers()
    {
        var coded = Fabric.Create(FabricId.From(3), "Сантана", MoneyAmount.From(50), SupplierId.From(5)).Value;
        coded.ApplyDetails(new FabricDetails("FAB-003", "Сантана", null, SupplierId.From(5), "100%поліестер", null, "90",
            true, null, null));
        var legacy = Fabric.Create(FabricId.From(7), "Тканина \"Грета\" Черкаси, ВВ", MoneyAmount.From(100), SupplierId.From(5)).Value;
        var stale = Fabric.Create(FabricId.From(9), "Стара тканина", MoneyAmount.From(10), SupplierId.From(5)).Value;
        var added = new List<Fabric>();
        var addedSuppliers = new List<Supplier>();
        var fabrics = Repository([coded, legacy, stale], added);
        var suppliers = Repository(Suppliers(), addedSuppliers);
        var handler = new ImportFabricsHandler(fabrics.Object, suppliers.Object);

        var result = await handler.Handle(new ImportFabricsCommand(
        [
            FabricRow("FAB-003", "Сантана", "Спецтекстиль", 56m, 4),
            FabricRow("fab-010", "Тканина «Грета» Черкаси ВВ", null, 100m, 5),
            FabricRow("FAB-011", "Флис", "Тексика", 120m, 6),
        ], DryRun: false, DeactivateMissing: true), CancellationToken.None);

        var value = result.Value;
        Assert.Multiple(() =>
        {
            Assert.That(value.Applied, Is.True);
            Assert.That((value.Created, value.Updated, value.Unchanged, value.Errors), Is.EqualTo((1, 2, 0, 0)));
            Assert.That(value.Rows[0].Changes, Is.EquivalentTo(new[] { "Склад", "Щільність", "Ціна", "Дата ціни" }));
            Assert.That(coded.Price.Value, Is.EqualTo(56m));
            Assert.That(coded.PriceUpdatedAt, Is.EqualTo(SheetDate));
            Assert.That(legacy.Code, Is.EqualTo("FAB-010"));
            Assert.That(legacy.ProviderId, Is.EqualTo(SupplierId.From(1)));
            Assert.That(added.Single().Id, Is.EqualTo(FabricId.From(10)));
            Assert.That(addedSuppliers.Single().Name, Is.EqualTo("Тексика"));
            Assert.That(added.Single().ProviderId, Is.EqualTo(addedSuppliers.Single().Id));
            Assert.That(stale.IsActive, Is.False);
            Assert.That(value.Deactivated, Is.EqualTo(new[] { "Стара тканина" }));
        });
        fabrics.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Fabric_preview_changes_nothing_and_repeating_the_sheet_is_unchanged()
    {
        var fabric = Fabric.Create(FabricId.From(1), new FabricDetails("FAB-001", "МВО", null, SupplierId.From(5),
            "100% бавовна", null, "260", true, null, null), MoneyAmount.From(147), SheetDate).Value;
        var added = new List<Fabric>();
        var fabrics = Repository([fabric], added);
        var handler = new ImportFabricsHandler(fabrics.Object, Repository(Suppliers(), []).Object);

        var result = await handler.Handle(new ImportFabricsCommand(
            [FabricRow("FAB-001", "МВО", "Спецтекстиль", 147m), FabricRow("FAB-002", "Бріз", "Спецтекстиль", 68m, 3)],
            DryRun: true, DeactivateMissing: false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Applied, Is.False);
            Assert.That(result.Value.Rows.Select(x => x.Action),
                Is.EqualTo(new[] { ReferenceImportActions.Unchanged, ReferenceImportActions.Create }));
            Assert.That(added, Is.Empty);
        });
        fabrics.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Accessory_row_error_stops_the_whole_import()
    {
        var added = new List<GarmentAccessory>();
        var accessories = Repository(new List<GarmentAccessory>(), added);
        var handler = new ImportGarmentAccessoriesHandler(accessories.Object, Repository(Suppliers(), []).Object);

        var result = await handler.Handle(new ImportGarmentAccessoriesCommand(
            [AccessoryRow("FUR-001", "Нитки", "шт"), AccessoryRow("FUR-002", "Тасьма", "кг", 3)],
            DryRun: false, DeactivateMissing: false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Value.Applied, Is.False);
            Assert.That(result.Value.Errors, Is.EqualTo(1));
            Assert.That(result.Value.Rows[1].Error, Is.EqualTo("Невідома одиниця «кг». Можна: шт., м, послуга."));
            Assert.That(added, Is.Empty);
        });
        accessories.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Accessory_import_keeps_sheet_unit_and_price_date()
    {
        var added = new List<GarmentAccessory>();
        var handler = new ImportGarmentAccessoriesHandler(
            Repository(new List<GarmentAccessory>(), added).Object, Repository(Suppliers(), []).Object);

        await handler.Handle(new ImportGarmentAccessoriesCommand(
            [AccessoryRow("FUR-011", "Стрічка", "м."), AccessoryRow("FUR-040", "Вишивка", "Послуга", 3)],
            DryRun: false, DeactivateMissing: false), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(added.Select(x => x.Unit), Is.EqualTo(new[] { "м", "послуга" }));
            Assert.That(added.Select(x => x.Code), Is.EqualTo(new[] { "FUR-011", "FUR-040" }));
            Assert.That(added.All(x => x.PriceUpdatedAt == SheetDate), Is.True);
        });
    }
}
