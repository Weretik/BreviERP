using Microsoft.EntityFrameworkCore;
using Npgsql;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements;
using Warehouse.Application.Features.Stock.Queries.GetFabricMovements.Results;
using Warehouse.Application.Features.Stock.Shared;
using Warehouse.Domain.Stock;
using Warehouse.Domain.Stock.Movements;
using Warehouse.Infrastructure.Persistence;

namespace IntegrationTests.Platform.Data;

/// <summary>
/// «Рух тканини» on PostgreSQL (database audit 2026-11-15, 1.4): the search, the kind counts and the page are SQL, and
/// the word search uses the trigram column of the ledger.
/// </summary>
[NonParallelizable]
public sealed class FabricMovementLedgerQueryTests
{
    private static readonly WarehouseActor Chief = new(4, false, false, true);
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 10, 0, 0, TimeSpan.Zero);
    private string _database = null!;
    private string _connection = null!;
    private string _maintenance = null!;

    [SetUp]
    public async Task CreateDatabase()
    {
        var baseConnection = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(baseConnection))
            Assert.Ignore("Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string.");
        _database = $"brevierp_ledger_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnection);
        _maintenance = new NpgsqlConnectionStringBuilder(builder.ConnectionString) { Database = "postgres" }.ConnectionString;
        builder.Database = _database;
        _connection = builder.ConnectionString;
        await MaintenanceAsync($"CREATE DATABASE \"{_database}\"");
    }

    [TearDown]
    public async Task DropDatabase()
    {
        NpgsqlConnection.ClearAllPools();
        await MaintenanceAsync($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)");
    }

    [Test]
    public async Task Search_counts_and_page_run_in_sql_with_the_pair_balance()
    {
        await using (var db = Context())
        {
            await db.Database.MigrateAsync();
            var grey = FabricStock.Create(new StockFabric(23, "FAB-023", "Грета"), "Темно-сірий", Now);
            var red = FabricStock.Create(new StockFabric(24, "FAB-024", "Саржа"), "Червоний", Now);
            db.FabricStocks.AddRange(grey, red);
            grey.Move(FabricMovementKind.Opening, 10m, new(new DateOnly(2026, 11, 1), null), "Начальник", Now);
            grey.Move(FabricMovementKind.Receipt, 5m, new(new DateOnly(2026, 11, 2), null, null, "Черкаси ВВ", "ТН-7"),
                "Начальник", Now);
            grey.Move(FabricMovementKind.Issue, -3m, new(new DateOnly(2026, 11, 3), null,
                Target: new FabricMovementTarget(1, 2, 14, "Костюм")), "Начальник", Now);
            red.Move(FabricMovementKind.Opening, 7m, new(new DateOnly(2026, 11, 1), "Залишок зі старого складу"),
                "Начальник", Now);
            await db.SaveChangesAsync();
        }

        var all = await PageAsync(FabricMovementFilter.All, null);
        var supplier = await PageAsync(FabricMovementFilter.All, "черкаси тн-7");
        var order = await PageAsync(FabricMovementFilter.ItemIssue, "ц-14");
        var colour = await PageAsync(FabricMovementFilter.All, "червоний старого");

        Assert.Multiple(() =>
        {
            Assert.That(all.Total, Is.EqualTo(4));
            Assert.That(all.Counts, Is.EqualTo(new FabricMovementCountsResponse(4, 1, 1, 0, 0, 2, 0)));
            Assert.That(all.Rows.Where(x => x.FabricCode == "FAB-023").Select(x => x.PairMetersAfter),
                Is.EqualTo(new decimal?[] { 12m, 15m, 10m }), "newest first, the running pair balance");
            Assert.That(supplier.Rows.Single().Kind, Is.EqualTo("Receipt"));
            Assert.That(order.Rows.Single().OrderNumber, Is.EqualTo(14));
            Assert.That(colour.Rows.Single().FabricCode, Is.EqualTo("FAB-024"));
        });
    }

    private async Task<FabricMovementPageResponse> PageAsync(FabricMovementFilter kind, string? search)
    {
        await using var db = Context();
        return (await new GetFabricMovementsHandler(db)
            .Handle(new(Chief, 1, kind, null, null, null, search), CancellationToken.None)).Value;
    }

    private WarehouseDbContext Context() => new(new DbContextOptionsBuilder<WarehouseDbContext>()
        .UseNpgsql(_connection, x => x.MigrationsHistoryTable("__EFMigrationsHistory", WarehouseDbContext.Schema))
        .Options);

    private async Task MaintenanceAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_maintenance);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
