using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UnitTests.Production;
using Warehouse.Application.Abstractions;
using Warehouse.Infrastructure.Adapters.Production;
using Warehouse.Infrastructure.Persistence;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Application.Features.Stock.Shared.Facts;
using Production.Application.Features.FabricPlans.Shared;
using Warehouse.Application.Features.Stock.Shared.Booking;
using Warehouse.Application.Features.Stock.Shared.Operations;
using Warehouse.Application.Features.Stock.Shared.Plans;

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

/// <summary>The stock facts for «Звіти цеху», mapped the way the host maps them.</summary>
internal sealed class HarnessProductionStockReader(StockFacts facts) : global::Production.Application.Abstractions.Materials.IProductionStockReader
{
    public async Task<IReadOnlyList<global::Production.Application.Abstractions.Materials.ProductionStockIssue>> IssuesAsync(
        DateOnly from, DateOnly to, CancellationToken cancellationToken) =>
        (await facts.IssuesAsync(from, to, cancellationToken)).Select(x =>
            new global::Production.Application.Abstractions.Materials.ProductionStockIssue(x.MovementId, x.OperationDate,
                x.FabricCode, x.FabricName, x.Color, x.OrderId, x.OrderNumber, x.ModelName, x.Meters)).ToArray();

    public async Task<IReadOnlyList<global::Production.Application.Abstractions.Materials.ProductionStockBalance>> BalancesAsync(
        DateOnly before, CancellationToken cancellationToken) =>
        (await facts.BalancesAsync(before, cancellationToken)).Select(x =>
            new global::Production.Application.Abstractions.Materials.ProductionStockBalance(x.StockId, x.FabricCode,
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
        Production = new WarehouseProductionReader(new ProductionFabricPlans(workshop.Db, workshop.Time,
            new ProductionAssemblies(workshop.Db, workshop.Models)));
    }

    public ProductionTestHarness Workshop { get; }
    public WarehouseDbContext Db { get; }
    public FakeWarehouseReferenceReader Reference { get; } = new();
    public IWarehouseStaffReader Staff { get; }
    public IWarehouseProductionReader Production { get; }

    public WarehouseColors Colors() => new(Reference);

    public StockReservations Reservations() => new(Db, Production, Plans());

    public ItemPlans Plans() => new(Db);

    public FabricBookingCalculator Booking() => new(Db, Production, Reference, Reservations(), Plans(), Workshop.Time);

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
