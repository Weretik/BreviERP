using Catalog.Domain.Products.Entities;
using Catalog.Infrastructure.DataBase;
using Catalog.Infrastructure.Repositories;
using CatalogImport.Execution;
using CatalogImport.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Reference.Domain.Suppliers.Entities;
using Reference.Domain.Suppliers.ValueObjects;
using Reference.Infrastructure.DataBase;

namespace IntegrationTests;

[NonParallelizable]
public sealed class CatalogImportPersistenceTests
{
    private string _connection = null!;
    private string _maintenance = null!;
    private string _database = null!;
    private string _source = null!;

    [OneTimeSetUp]
    public async Task CreateDatabase()
    {
        var configured = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(configured, Is.Not.Null.And.Not.Empty, "A local PostgreSQL test connection is required.");
        var builder = new NpgsqlConnectionStringBuilder(configured);
        _database = $"brevi_catalog_import_{Guid.NewGuid():N}";
        _maintenance = new NpgsqlConnectionStringBuilder(configured) { Database = "postgres" }.ConnectionString;
        await using var connection = new NpgsqlConnection(_maintenance);
        await connection.OpenAsync();
        await using var create = new NpgsqlCommand($"CREATE DATABASE \"{_database}\"", connection);
        await create.ExecuteNonQueryAsync();
        builder.Database = _database;
        _connection = builder.ConnectionString;
        _source = Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures", "catalog-import.json");
        await using var reference = Reference();
        await reference.Database.MigrateAsync();
        reference.Suppliers.Add(Supplier.Create(SupplierId.From(1), "Не указан"));
        await reference.SaveChangesAsync();
        await using var catalog = Catalog();
        await catalog.Database.MigrateAsync();
        await using var provenance = Provenance();
        await provenance.Database.MigrateAsync();
    }

    [OneTimeTearDown]
    public async Task DropDatabase()
    {
        if (string.IsNullOrEmpty(_database))
            return;
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_maintenance);
        await connection.OpenAsync();
        await using var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_database}\" WITH (FORCE)", connection);
        await drop.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task Trial_rolls_back_apply_preserves_source_and_retry_is_idempotent()
    {
        var validation = await CatalogImportJob.RunAsync(_connection, _source, "validate");
        Assert.That(validation.Status, Is.EqualTo("validated"));
        var trial = await CatalogImportJob.RunAsync(_connection, _source, "trial");
        Assert.That(trial.Status, Is.EqualTo("trial-rolled-back"));
        await using (var catalog = Catalog())
            Assert.That(await catalog.Products.CountAsync(), Is.Zero);
        await using (var reference = Reference())
            Assert.That(await reference.Suppliers.CountAsync(), Is.EqualTo(1));
        await using (var provenance = Provenance())
            Assert.That(await provenance.Batches.CountAsync(), Is.Zero);

        var applied = await CatalogImportJob.RunAsync(_connection, _source, "apply");
        Assert.Multiple(() =>
        {
            Assert.That(applied.Status, Is.EqualTo("applied"));
            Assert.That(applied.Products, Is.EqualTo(2));
            Assert.That(applied.Articles, Is.EqualTo(2));
            Assert.That(applied.Materials, Is.EqualTo(3));
            Assert.That(applied.Operations, Is.EqualTo(2));
        });
        await using (var catalog = Catalog())
        {
            var profile = await catalog.Set<ProductCatalogProfile>().SingleAsync(x => x.SaleSku == "724");
            Assert.That(profile.SupplierPrice, Is.Null);
            Assert.That(await new SewingCompositionStore(catalog).SaleSkuExistsAsync("724", CancellationToken.None), Is.True);
            var material = await catalog.Set<SewingMaterialLine>().SingleAsync(x => x.ArticleId == null);
            Assert.That(material.Quantity, Is.EqualTo(1.9m));
            Assert.That(material.Unit.ToString(), Is.EqualTo("Metre"));
            Assert.That(await catalog.Set<SewingOperationLine>().OrderBy(x => x.SortOrder)
                .Select(x => x.ManualMinutes).ToListAsync(), Is.EqualTo(new decimal?[] { 20m, 86m }));
        }
        var retry = await CatalogImportJob.RunAsync(_connection, _source, "apply");
        Assert.That(retry.Status, Is.EqualTo("already-applied"));
        await using (var catalog = Catalog())
            Assert.That(await catalog.Products.CountAsync(), Is.EqualTo(2));
        await using (var provenance = Provenance())
        {
            Assert.That(await provenance.Batches.CountAsync(), Is.EqualTo(1));
            Assert.That(await provenance.Mappings.CountAsync(), Is.GreaterThan(5));
            Assert.That((await provenance.Batches.SingleAsync()).SourceJson, Does.Contain("test-workbook"));
        }

        var modified = Path.Combine(TestContext.CurrentContext.WorkDirectory, $"catalog-import-{Guid.NewGuid():N}.json");
        try
        {
            await File.WriteAllTextAsync(modified, (await File.ReadAllTextAsync(_source)).Replace("Test model", "Changed model"));
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await CatalogImportJob.RunAsync(_connection, modified, "apply"));
        }
        finally
        {
            File.Delete(modified);
        }
    }

    private CatalogDbContext Catalog() => new(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(_connection).Options);
    private ReferenceDbContext Reference() => new(new DbContextOptionsBuilder<ReferenceDbContext>().UseNpgsql(_connection).Options);
    private CatalogImportDbContext Provenance() => new(new DbContextOptionsBuilder<CatalogImportDbContext>().UseNpgsql(_connection).Options);
}

