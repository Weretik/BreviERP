using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Production.Application.FabricPlans;
using UnitTests.Production;
using Warehouse.Application.Abstractions;
using Warehouse.Application.Colors;
using Warehouse.Application.Stock;
using Warehouse.Application.Stock.Facts;
using Warehouse.Application.Stock.Issues;
using Warehouse.Infrastructure.Persistence;

namespace UnitTests.Warehouse;

internal sealed class FakeWarehouseReferenceReader : IWarehouseReferenceReader
{
    public List<WarehouseFabric> Fabrics { get; } =
    [
        new(23, "FAB-023", "Тканина «Грета»", 120m, "53% бавовна, 47% поліестер", "220", "Тексика"),
        new(67, "FAB-067", "Тканина «Грета»", 110m, "53 % бавовна, 47 % поліестер", "220 г/м²", "Черкаси ВВ"),
        new(5, "FAB-005", "Оксфорд", 90m, "100% поліестер", "150", "Тексика")
    ];
    public List<WarehouseSupplier> Suppliers { get; } = [new(7, "Тексика")];
    public List<string> Colors { get; } = ["Темно-сірий", "Червоний", "Жовтий", "Сірий"];

    public Task<IReadOnlyList<WarehouseFabric>> FabricsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<WarehouseFabric>>(Fabrics);

    public Task<IReadOnlyList<WarehouseSupplier>> SuppliersAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<WarehouseSupplier>>(Suppliers);

    public Task<IReadOnlyList<string>> ColorsAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(Colors);
}

/// <summary>The workshop staff of the Production harness.</summary>
internal sealed class HarnessWarehouseStaffReader(ProductionTestHarness workshop) : IWarehouseStaffReader
{
    public async Task<WarehouseStaffMember?> FindAsync(int userId, CancellationToken cancellationToken) =>
        await workshop.Staff.FindAsync(userId, cancellationToken) is { } x ? new(x.UserId, x.Name, x.Email) : null;

    public async Task<WarehouseStaffMember?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        await workshop.Staff.FindByEmailAsync(email, cancellationToken) is { } x ? new(x.UserId, x.Name, x.Email) : null;
}

/// <summary>The real Production plans over the Production harness, mapped the way the host maps them.</summary>
internal sealed class HarnessWarehouseProductionReader(ProductionFabricPlans plans,
    ProductionAssemblies assemblies) : IWarehouseProductionReader
{
    public async Task<IReadOnlyList<WarehousePlanItem>> OpenItemsAsync(string? fabricCode,
        CancellationToken cancellationToken) =>
        (await plans.OpenAsync(fabricCode, cancellationToken)).Select(Map).ToArray();

    public async Task<WarehousePlanItem?> ItemAsync(int orderId, int itemId, CancellationToken cancellationToken) =>
        await plans.ItemAsync(orderId, itemId, cancellationToken) is { } item ? Map(item) : null;

    public async Task<IReadOnlyList<WarehousePlanItem>> ItemsAsync(IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken) =>
        (await plans.ItemsAsync(itemIds, cancellationToken)).Select(Map).ToArray();

    public async Task<IReadOnlyList<WarehouseAssemblyItem>> OpenAssembliesAsync(CancellationToken cancellationToken) =>
        (await assemblies.OpenAsync(cancellationToken)).Select(x => new WarehouseAssemblyItem(x.OrderId, x.OrderNumber,
            x.ClientName, x.OrderCreatedAt, x.ItemId, x.ModelName, x.ModelCode, x.Quantity,
            x.Lines?.Select(l => new WarehouseAssemblyLine(l.Kind, l.Code, l.Name, l.Quantity, l.Unit, l.Comment))
                .ToArray())).ToArray();

    public async Task<IReadOnlyList<WarehouseLegacyOrder>> LegacyOrdersAsync(IReadOnlyCollection<string> legacyIds,
        CancellationToken cancellationToken) =>
        (await plans.LegacyAsync(legacyIds, cancellationToken))
        .Select(x => new WarehouseLegacyOrder(x.LegacyId, x.OrderId, x.Number,
            x.Items.Select(i => new WarehouseLegacyItem(i.Position, i.ItemId, i.ModelName, i.ModelCode, i.Quantity)).ToArray()))
        .ToArray();

    public Task RecordFabricAsync(int orderId, int itemId, string text, string actorName,
        CancellationToken cancellationToken) =>
        plans.RecordFabricAsync(orderId, itemId, text, actorName, cancellationToken);

    private static WarehousePlanItem Map(ProductionPlanItem x) => new(x.OrderId, x.OrderNumber, x.OrderVersion,
        x.IsOpen, x.ClientName, x.ItemId, x.ModelName, x.FabricCode, x.FabricName, x.Quantity, x.HasNorm,
        x.Parts.Select(p => new WarehousePlanPart(p.FabricCode, p.Color, p.ColorKey, p.Meters)).ToArray(), x.ModelCode,
        x.OrderCreatedAt, x.DueDate);
}

/// <summary>The stock facts for «Звіти цеху», mapped the way the host maps them.</summary>
internal sealed class HarnessProductionStockReader(StockFacts facts) : global::Production.Application.Abstractions.IProductionStockReader
{
    public async Task<IReadOnlyList<global::Production.Application.Abstractions.ProductionStockIssue>> IssuesAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken) =>
        (await facts.IssuesAsync(from, to, cancellationToken)).Select(x =>
            new global::Production.Application.Abstractions.ProductionStockIssue(x.MovementId, x.OperationDate,
                x.FabricCode, x.FabricName, x.Color, x.OrderId, x.OrderNumber, x.ModelName, x.Meters)).ToArray();

    public async Task<IReadOnlyList<global::Production.Application.Abstractions.ProductionStockBalance>> BalancesAsync(
        DateOnly before, CancellationToken cancellationToken) =>
        (await facts.BalancesAsync(before, cancellationToken)).Select(x =>
            new global::Production.Application.Abstractions.ProductionStockBalance(x.StockId, x.FabricCode,
                x.FabricName, x.Color, x.Meters)).ToArray();
}

/// <summary>Warehouse module on SQLite in memory next to a Production harness with its orders and norms.</summary>
internal sealed class WarehouseTestHarness : IDisposable
{
    private readonly SqliteConnection _connection;

    private readonly bool _ownsWorkshop;

    public WarehouseTestHarness(ProductionTestHarness workshop, bool ownsWorkshop = true)
    {
        Workshop = workshop;
        _ownsWorkshop = ownsWorkshop;
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        Db = new WarehouseDbContext(new DbContextOptionsBuilder<WarehouseDbContext>().UseSqlite(_connection).Options);
        Db.Database.EnsureCreated();
        Staff = new HarnessWarehouseStaffReader(workshop);
        Production = new HarnessWarehouseProductionReader(new ProductionFabricPlans(workshop.Db, workshop.Time),
            new ProductionAssemblies(workshop.Db, workshop.Models));
    }

    public ProductionTestHarness Workshop { get; }
    public WarehouseDbContext Db { get; }
    public FakeWarehouseReferenceReader Reference { get; } = new();
    public IWarehouseStaffReader Staff { get; }
    public IWarehouseProductionReader Production { get; }

    public WarehouseColors Colors() => new(Reference);

    public StockReservations Reservations() => new(Db, Production, Plans());

    public ItemPlans Plans() => new(Db);

    public StockChanges StockChanges() => new(Db, Staff, Reference, Workshop.Time);

    public StockOperations Operations() => new(Db);

    public StockPairs Pairs() => new(Db, Operations(), Workshop.Time);

    public ItemFabricCards ItemFabric() => new(Db, Reservations(), Plans());

    public StockFacts Facts() => new(Db);

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
        if (_ownsWorkshop) Workshop.Dispose();
    }
}
