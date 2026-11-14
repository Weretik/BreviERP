using Ardalis.Specification.EntityFrameworkCore;
using Catalog.Domain.Media;
using Catalog.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Application.Features.Product.Shared.Photos;
using Catalog.Infrastructure.Persistence;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.Photos;
using Catalog.Domain.Products.ValueObjects;

namespace IntegrationTests.Catalog.Products;

/// <summary>The tab «Фото», the list thumbnail and the SKU index on PostgreSQL (CAT-016).</summary>
[NonParallelizable]
public sealed class ProductPhotoPersistenceTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 4, 22, 0, 0, TimeSpan.Zero);
    private string _databaseName = null!;
    private string _testConnectionString = null!;
    private string _maintenanceConnectionString = null!;

    [OneTimeSetUp]
    public async Task CreateTestDatabase()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(baseConnectionString, Is.Not.Null.And.Not.Empty,
            "Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string with CREATE DATABASE permission.");
        _databaseName = $"brevierp_product_photos_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
        _maintenanceConnectionString = new NpgsqlConnectionStringBuilder(builder.ConnectionString) { Database = "postgres" }.ConnectionString;
        builder.Database = _databaseName;
        _testConnectionString = builder.ConnectionString;
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
        if (string.IsNullOrWhiteSpace(_databaseName) || string.IsNullOrWhiteSpace(_maintenanceConnectionString)) return;
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    [Test]
    public async Task Photo_list_change_is_saved_and_the_list_shows_the_main_visible_photo()
    {
        await using (var db = CreateContext())
        {
            var product = Product.Create(ProductId.Create(901), "Куртка «Тест»", "Куртка «Тест»",
                ProductSlug.Create("kurtka-test").Value, ProductType.Sewing, Now).Value;
            product.AddPhoto(ProductPhotoId.Create(1), MediaFileId.Create(10), Now, sortOrder: 0);
            product.AddPhoto(ProductPhotoId.Create(2), MediaFileId.Create(20), Now, sortOrder: 1);
            db.Products.Add(product);
            await db.SaveChangesAsync();
        }

        await using (var db = CreateContext())
        {
            var product = await SpecificationEvaluator.Default
                .GetQuery(db.Products.AsQueryable(), new ProductPhotosByIdSpec(901, true)).SingleAsync();
            product.SetPhotoList([MediaFileId.Create(30), MediaFileId.Create(20)], MediaFileId.Create(20), Now);
            await db.SaveChangesAsync();
        }

        await using (var db = CreateContext())
        {
            var row = await SpecificationEvaluator.Default
                .GetQuery(db.Products.AsQueryable(), new ProductOverviewSpec(null, 901)).SingleAsync();
            var index = await SpecificationEvaluator.Default
                .GetQuery(db.Products.AsQueryable(), new ProductSkuIndexSpec()).ToListAsync();
            var photos = await db.Set<ProductPhoto>().AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(photos.Select(x => x.MediaFileId.Value), Is.EqualTo(new[] { 30, 20 }));
                Assert.That(row.MainPhotoFileId, Is.EqualTo(20));
                Assert.That(index.Single(x => x.Id == 901).ArticleSkus, Is.Empty);
            });
        }
    }

    private CatalogDbContext CreateContext()
        => new(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(_testConnectionString).Options);
}
