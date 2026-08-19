using Ardalis.Result;
using Ardalis.Specification.EntityFrameworkCore;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Product.Delete;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace IntegrationTests;

[NonParallelizable]
public sealed class ProductDeletionTests
{
    private string _databaseName = null!;
    private string _testConnectionString = null!;
    private string _maintenanceConnectionString = null!;

    [OneTimeSetUp]
    public async Task CreateTestDatabase()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(baseConnectionString, Is.Not.Null.And.Not.Empty,
            "Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string with CREATE DATABASE permission.");

        _databaseName = $"brevierp_catalog_product_delete_{Guid.NewGuid():N}";
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

        await using var db = CreateContext();
        await db.Database.MigrateAsync();
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
    public async Task Handler_deletes_product_and_owned_photo_but_preserves_media_file()
    {
        const int productId = 701;
        const int mediaFileId = 801;
        await using var db = CreateContext();
        var mediaFile = MediaFile.CreatePending(
            MediaFileId.Create(mediaFileId), "product.jpg", "image/jpeg", 1024,
            "test", "catalog", "products/product.jpg");
        var product = Product.Create(ProductId.Create(productId), "Product", "Товар",
            ProductSlug.Create("product"), ProductType.Sewing, DateTimeOffset.UtcNow);
        product.AddPhoto(ProductPhotoId.Create(1), mediaFile.Id, DateTimeOffset.UtcNow);
        db.AddRange(mediaFile, product);
        await db.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(new TestCatalogRepository(db));
        var result = await handler.Handle(new DeleteProductCommand(productId), CancellationToken.None);

        db.ChangeTracker.Clear();
        Assert.That(result.Status, Is.EqualTo(ResultStatus.Ok));
        Assert.That(await db.Products.AnyAsync(x => x.Id == ProductId.Create(productId)), Is.False);
        Assert.That(await db.Set<ProductPhoto>().AnyAsync(x => x.ProductId == ProductId.Create(productId)), Is.False);
        Assert.That(await db.MediaFiles.AnyAsync(x => x.Id == MediaFileId.Create(mediaFileId)), Is.True);
    }

    [Test]
    public async Task Handler_returns_not_found_when_product_does_not_exist()
    {
        await using var db = CreateContext();
        var handler = new DeleteProductCommandHandler(new TestCatalogRepository(db));

        var result = await handler.Handle(new DeleteProductCommand(999_999), CancellationToken.None);

        Assert.That(result.Status, Is.EqualTo(ResultStatus.NotFound));
    }

    private CatalogDbContext CreateContext()
        => new(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_testConnectionString)
            .Options);

    private sealed class TestCatalogRepository(CatalogDbContext db)
        : RepositoryBase<Product>(db), ICatalogRepository<Product>;
}
