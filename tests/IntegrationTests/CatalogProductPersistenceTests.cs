using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using BuildingBlocks.Application.Behaviors;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Features.Product.Create;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Application.Features.Product.GetAdminDetail.Specifications;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntegrationTests;

[NonParallelizable]
public sealed class CatalogProductPersistenceTests
{
    private const string PreviousMigration = "20260727004045_InitialCatalog";
    private string _databaseName = null!;
    private string _testConnectionString = null!;
    private string _maintenanceConnectionString = null!;

    [OneTimeSetUp]
    public async Task CreateTestDatabase()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(baseConnectionString, Is.Not.Null.And.Not.Empty,
            "Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string with CREATE DATABASE permission.");

        _databaseName = $"brevierp_catalog_phase02_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
        _maintenanceConnectionString = new NpgsqlConnectionStringBuilder(builder.ConnectionString)
        {
            Database = "postgres"
        }.ConnectionString;
        builder.Database = _databaseName;
        _testConnectionString = builder.ConnectionString;

        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", connection);
        await command.ExecuteNonQueryAsync();
    }

    [OneTimeTearDown]
    public async Task DropTestDatabase()
    {
        if (string.IsNullOrWhiteSpace(_databaseName) || string.IsNullOrWhiteSpace(_maintenanceConnectionString))
            return;

        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            $"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task Migration_maps_constraints_persists_product_and_rolls_back_to_initial_catalog()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM catalog.\"Products\"");

        await AssertSchemaContainsPhaseTwoTables(db);
        await AssertProductLifecycleAndConstraints(db);

        var migrator = db.Database.GetService<IMigrator>();
        await migrator.MigrateAsync(PreviousMigration);

        await Assert.ThatAsync(
            () => db.Database.SqlQueryRaw<string>("SELECT to_regclass('catalog.\"SewingProductDetails\"')::text AS \"Value\"").SingleAsync(),
            Is.Null);
        await Assert.ThatAsync(
            () => db.Database.SqlQueryRaw<string>("SELECT to_regclass('catalog.\"Products\"')::text AS \"Value\"").SingleAsync(),
            Is.EqualTo("catalog.\"Products\""));
    }

    [Test]
    public async Task Unique_constraint_race_is_mapped_to_conflict_by_the_command_pipeline()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        var now = DateTimeOffset.UtcNow;
        db.Products.Add(Product.Create(ProductId.Create(201), "Concurrent product", "Конкурентний товар",
            ProductSlug.Create("concurrent-product"), ProductType.Sewing, now));
        await db.SaveChangesAsync();

        var behavior = new ExceptionBehavior<CreateProductCommand, Result<ProductAdminDetail>>(
            NullLogger<ExceptionBehavior<CreateProductCommand, Result<ProductAdminDetail>>>.Instance);

        var result = await behavior.Handle(
            new CreateProductCommand(null!),
            async (_, cancellationToken) =>
            {
                db.Products.Add(Product.Create(ProductId.Create(202), "Concurrent product", "Інший товар",
                    ProductSlug.Create("another-product"), ProductType.Sewing, now));
                await db.SaveChangesAsync(cancellationToken);
                return Result.Success<ProductAdminDetail>(null!);
            },
            CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.Conflict));
        await db.Database.ExecuteSqlRawAsync("DELETE FROM catalog.\"Products\"");
    }

    [Test]
    public async Task Admin_detail_projection_is_translated_to_a_no_tracking_sql_query()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM catalog.\"Products\"");
        var now = DateTimeOffset.UtcNow;
        var product = Product.Create(ProductId.Create(301), "Projection product", "Проекційний товар",
            ProductSlug.Create("projection-product"), ProductType.Sewing, now);
        product.ConfigureSewing(SewingProductDetails.Create(product.Id, 1m,
            [new ProductFabric(product.Id, 501, true, 0)], [], []), now);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var query = SpecificationEvaluator.Default.GetQuery(db.Products.AsQueryable(), new ProductAdminDetailSpec(301));
        var detail = await query.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(detail, Is.TypeOf<ProductAdminDetailReadModel>());
            Assert.That(detail.Sewing!.Fabrics.Single().FabricId, Is.EqualTo(501));
            Assert.That(query.ToQueryString(), Does.Contain("SELECT").And.Contain("Products"));
        });
        await db.Database.ExecuteSqlRawAsync("DELETE FROM catalog.\"Products\"");
    }

    private CatalogDbContext CreateContext()
        => new(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_testConnectionString)
            .Options);

    private static async Task AssertSchemaContainsPhaseTwoTables(CatalogDbContext db)
    {
        var tables = await db.Database.SqlQueryRaw<string>("""
            SELECT table_name
            FROM information_schema.tables
            WHERE table_schema = 'catalog'
              AND table_name IN (
                'SewingProductDetails', 'SewingProductFabrics', 'SewingProductAccessories',
                'SewingProductOperations', 'PpeProductDetails', 'ProductInformationBlocks',
                'ProductCharacteristicTables', 'ProductCharacteristicRows')
            """).ToListAsync();

        Assert.That(tables, Is.EquivalentTo(new[]
        {
            "SewingProductDetails", "SewingProductFabrics", "SewingProductAccessories",
            "SewingProductOperations", "PpeProductDetails", "ProductInformationBlocks",
            "ProductCharacteristicTables", "ProductCharacteristicRows"
        }));
    }

    private static async Task AssertProductLifecycleAndConstraints(CatalogDbContext db)
    {
        var id = ProductId.Create(101);
        var product = Product.Create(id, "Integration Sewing", "Интеграция Швейный",
            ProductSlug.Create("integration-sewing"), ProductType.Sewing, DateTimeOffset.UtcNow);
        product.ConfigureSewing(SewingProductDetails.Create(id, 1.25m,
            [new ProductFabric(id, 501, true, 0)], [], []), DateTimeOffset.UtcNow);

        db.Products.Add(product);
        await db.SaveChangesAsync();

        Assert.That(await db.Set<SewingProductDetails>().CountAsync(), Is.EqualTo(1));
        Assert.That(await db.Set<ProductFabric>().CountAsync(), Is.EqualTo(1));

        db.Products.Add(Product.Create(ProductId.Create(102), "Integration Sewing", "Інтеграція Дублікат",
            ProductSlug.Create("integration-sewing-duplicate"), ProductType.Sewing, DateTimeOffset.UtcNow));
        await Assert.ThatAsync(() => db.SaveChangesAsync(), Throws.TypeOf<DbUpdateException>());
        db.ChangeTracker.Clear();

        await Assert.ThatAsync(
            () => db.Database.ExecuteSqlRawAsync("""
                INSERT INTO catalog."SewingProductFabrics" ("ProductId", "FabricId", "IsPrimary", "SortOrder")
                VALUES (101, 501, FALSE, 1)
                """),
            Throws.TypeOf<PostgresException>());
        await Assert.ThatAsync(
            () => db.Database.ExecuteSqlRawAsync("""
                INSERT INTO catalog."PpeProductDetails"
                    ("ProductId", "SupplierId", "BasePrice", "RetailPercentSource", "WholesalePercentSource")
                VALUES (999, 1, 1, 'Reference', 'Reference')
                """),
            Throws.TypeOf<PostgresException>());

        await db.Database.ExecuteSqlRawAsync("DELETE FROM catalog.\"Products\" WHERE \"Id\" = 101");
        Assert.That(await db.Set<SewingProductDetails>().CountAsync(), Is.Zero);
        Assert.That(await db.Set<ProductFabric>().CountAsync(), Is.Zero);
    }
}
