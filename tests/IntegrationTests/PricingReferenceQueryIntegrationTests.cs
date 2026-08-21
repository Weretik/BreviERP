using Ardalis.Specification.EntityFrameworkCore;
using BuildingBlocks.Domain.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Data.Common;
using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.PricingReference.GetByIds;
using Reference.Domain.AdditionalReferences.Entities;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.Entities;
using Reference.Domain.GarmentPartOperations.Entities;
using Reference.Infrastructure.DataBase;

namespace IntegrationTests;

[NonParallelizable]
public sealed class PricingReferenceQueryIntegrationTests
{
    private string _databaseName = null!;
    private string _testConnectionString = null!;
    private string _maintenanceConnectionString = null!;

    [OneTimeSetUp]
    public async Task CreateTestDatabase()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(baseConnectionString, Is.Not.Null.And.Not.Empty);
        _databaseName = $"brevierp_pricing_reference_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = _databaseName };
        _testConnectionString = builder.ConnectionString;
        builder.Database = "postgres";
        _maintenanceConnectionString = builder.ConnectionString;

        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", connection);
        await command.ExecuteNonQueryAsync();
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task DropTestDatabase()
    {
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task Additional_reference_projection_reads_only_requested_ids_or_keys()
    {
        await using var db = CreateContext();
        db.AdditionalReferences.AddRange(
            AdditionalReference.Create(AdditionalReferenceId.From(10), "Profit", "profit_10", 30m, "%"),
            AdditionalReference.Create(AdditionalReferenceId.From(20), "Unused", "unused", 99m, "%"));
        await db.SaveChangesAsync();

        var handler = new GetPricingReferencesByIdsQueryHandler(
            new TestReferenceReadRepository<Fabric>(db),
            new TestReferenceReadRepository<GarmentAccessory>(db),
            new TestReferenceReadRepository<GarmentPartOperation>(db),
            new TestReferenceReadRepository<AdditionalReference>(db));

        var result = await handler.Handle(
            new GetPricingReferencesByIdsQuery([], [], [], [10], ["profit_10"]),
            CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Fabrics, Is.Empty);
            Assert.That(result.GarmentAccessories, Is.Empty);
            Assert.That(result.GarmentPartOperations, Is.Empty);
            Assert.That(result.AdditionalReferences, Is.EqualTo([new PricingReferenceItem(10, 30m, "profit_10", "%")]));
        });
    }

    [TestCase(1)]
    [TestCase(20)]
    [TestCase(50)]
    public async Task Pricing_reader_uses_a_fixed_number_of_selects_regardless_of_page_id_count(int pageSize)
    {
        var commandCounter = new SelectCommandCounter();
        await using var db = CreateContext(commandCounter);
        var ids = Enumerable.Range(1, pageSize).ToArray();
        var handler = new GetPricingReferencesByIdsQueryHandler(
            new TestReferenceReadRepository<Fabric>(db),
            new TestReferenceReadRepository<GarmentAccessory>(db),
            new TestReferenceReadRepository<GarmentPartOperation>(db),
            new TestReferenceReadRepository<AdditionalReference>(db));

        await handler.Handle(
            new GetPricingReferencesByIdsQuery(ids, ids, ids, ids, []),
            CancellationToken.None);

        Assert.That(commandCounter.SelectCount, Is.EqualTo(4));
    }

    private ReferenceDbContext CreateContext(DbCommandInterceptor? commandInterceptor = null)
    {
        var options = new DbContextOptionsBuilder<ReferenceDbContext>()
            .UseNpgsql(_testConnectionString);

        if (commandInterceptor is not null)
            options.AddInterceptors(commandInterceptor);

        return new ReferenceDbContext(options.Options);
    }

    private sealed class TestReferenceReadRepository<T>(ReferenceDbContext db)
        : RepositoryBase<T>(db), IReferenceReadRepository<T>
        where T : class, IAggregateRoot
    {
    }

    private sealed class SelectCommandCounter : DbCommandInterceptor
    {
        public int SelectCount { get; private set; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            if (command.CommandText.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                SelectCount++;

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }
    }
}
