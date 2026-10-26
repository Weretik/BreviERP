using System.Security.Cryptography;
using System.Text.Json;
using Catalog.Application.Features.Pricing.Sewing.Preview;
using Catalog.Domain.Pricing.Sewing;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Pricing.Reading;
using CatalogImport.Execution;
using CatalogImport.Persistence;
using SewingPricing.Jobs.Bootstrap;
using SewingPricing.Jobs.Reading;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Application.Contracts.Pricing;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.Pricing;
using Reference.Domain.Suppliers.Entities;
using Reference.Domain.Suppliers.ValueObjects;
using Reference.Infrastructure.DataBase;
using Reference.Infrastructure.Pricing.Reading;

namespace IntegrationTests.Pricing.Sewing;

public abstract class SewingPricingDatabaseFixture
{
    protected string _connection = null!;
    protected string _maintenance = null!;
    protected string _database = null!;
    protected string _pricingSource = null!;
    protected static readonly DateTimeOffset EffectiveFrom = new(2026, 10, 25, 0, 0, 0, TimeSpan.Zero);

    [OneTimeSetUp]
    public async Task CreateDatabaseAndImportedCatalogue()
    {
        var configured = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(configured, Is.Not.Null.And.Not.Empty, "A local PostgreSQL test connection is required.");
        _database = $"brevi_sewing_pricing_{Guid.NewGuid():N}";
        _maintenance = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres" }.ConnectionString;
        _connection = new NpgsqlConnectionStringBuilder(configured) { Database = _database }.ConnectionString;
        await using var connection = new NpgsqlConnection(_maintenance);
        await connection.OpenAsync();
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", connection);
        await create.ExecuteNonQueryAsync();
        await using var reference = Reference();
        await reference.Database.MigrateAsync();
        reference.Suppliers.Add(Supplier.Create(SupplierId.From(1), "Не указан"));
        await reference.SaveChangesAsync();
        await using var catalog = Catalog();
        await catalog.Database.MigrateAsync();
        await using var provenance = new CatalogImportDbContext(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(_connection).Options);
        await provenance.Database.MigrateAsync();
        var source = CatalogueSourcePath();
        await CatalogImportJob.RunAsync(_connection, source, "apply");
        var hash = Convert.ToHexString(SHA256.HashData(await File.ReadAllBytesAsync(source)));
        _pricingSource = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"pricing-{Guid.NewGuid():N}.json");
        var codes = new Dictionary<string, decimal>
        {
            ["SALARY_SEAMSTRESS"] = 17500,
            ["WORK_DAYS_MONTH"] = 20.5m,
            ["MINUTES_SHIFT"] = 480,
            ["SEAMSTRESS_COUNT"] = 12,
            ["ASSEMBLY_RATE"] = .25m,
            ["CUTTING_RATE"] = .2174m,
            ["MASTER_RATE"] = .11m,
            ["SHOP_MANAGER_RATE"] = .12m,
            ["SEAMSTRESS_BONUS"] = .08m,
            ["MONTHLY_OVERHEAD"] = 210000,
            ["SEWING_PRICE_COEF_1"] = 1.55m,
            ["SEWING_PRICE_COEF_2"] = 1.32m,
            ["SEWING_PRICE_COEF_3"] = 1.25m
        };
        var rows = new List<object?[]> { new object?[] { "Group", "Code", "Name", "Value" } };
        rows.AddRange(codes.Select(pair => new object?[] { null, pair.Key, pair.Key, pair.Value }));
        await File.WriteAllTextAsync(_pricingSource, JsonSerializer.Serialize(new
        {
            version = 1,
            spreadsheetId = "test-workbook",
            capturedAt = EffectiveFrom,
            catalogSnapshotHash = hash,
            settings = rows,
            formulas = new { minuteCost = "salary/(days*shift)" }
        }));
    }

    [OneTimeTearDown]
    public async Task DropDatabase()
    {
        if (!string.IsNullOrEmpty(_pricingSource))
            File.Delete(_pricingSource);
        if (string.IsNullOrEmpty(_database))
            return;
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_maintenance);
        await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)", connection);
        await drop.ExecuteNonQueryAsync();
    }

    protected virtual string CatalogueSourcePath() => Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "catalog-import.json");

    protected CatalogDbContext Catalog() => new(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(_connection).Options);
    protected ReferenceDbContext Reference() => new(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(_connection).Options);
}

