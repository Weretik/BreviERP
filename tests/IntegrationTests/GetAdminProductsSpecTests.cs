using System.Data.Common;
using Ardalis.Specification.EntityFrameworkCore;
using Catalog.Application.Features.Product.GetAdminList;
using Catalog.Application.Features.Product.GetAdminList.Specifications;
using Catalog.Domain.ProductCategories.Entities;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using Catalog.Infrastructure.DataBase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql;

namespace IntegrationTests;

[NonParallelizable]
public sealed class GetAdminProductsSpecTests
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

        _databaseName = $"brevierp_catalog_admin_list_{Guid.NewGuid():N}";
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
    public async Task Specification_filters_descendant_categories_searches_all_name_tokens_sorts_and_pages_in_sql()
    {
        await using var db = CreateContext();
        var root = ProductCategory.Create(ProductCategoryId.Create(1), "Одяг", "Одежда", "odiah");
        var child = ProductCategory.Create(ProductCategoryId.Create(2), "Куртки", "Куртки", "kurtky", root.Id, root.Path);
        var other = ProductCategory.Create(ProductCategoryId.Create(3), "ЗІЗ", "СИЗ", "ziz");
        db.ProductCategories.AddRange(root, child, other);

        db.Products.AddRange(
            CreateProduct(10, "Alpha Product One", "alpha-product-one", ProductType.Sewing, child.Id),
            CreateProduct(20, "Alpha Product Two", "alpha-product-two", ProductType.Ppe, root.Id),
            CreateProduct(30, "Gamma Product", "gamma-product", ProductType.Sewing, other.Id));
        await db.SaveChangesAsync();

        var filterSpec = new GetAdminProductsSpec("alp prod", null, [root.Id, child.Id], "id", false);
        var pageSpec = new GetAdminProductsSpec("alp prod", null, [root.Id, child.Id], "id", false, 2, 1);

        var filteredQuery = SpecificationEvaluator.Default.GetQuery(db.Products.AsQueryable(), filterSpec);
        var pageQuery = SpecificationEvaluator.Default.GetQuery(db.Products.AsQueryable(), pageSpec);
        var total = await filteredQuery.CountAsync();
        var items = await pageQuery.ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(total, Is.EqualTo(2));
            Assert.That(items.Select(x => x.Id), Is.EqualTo(new[] { 20 }));
            Assert.That(pageQuery.ToQueryString(), Does.Contain("LIMIT").And.Contain("OFFSET"));
        });
    }

    [Test]
    public async Task Specification_uses_id_as_a_stable_secondary_sort_for_paged_results()
    {
        await using var db = CreateContext();
        var category = ProductCategory.Create(ProductCategoryId.Create(4), "Взуття", "Обувь", "vzuttia");
        var createdAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        db.ProductCategories.Add(category);
        db.Products.AddRange(
            CreateProduct(130, "Товар 130", "product-130", ProductType.Sewing, category.Id, createdAt),
            CreateProduct(110, "Товар 110", "product-110", ProductType.Sewing, category.Id, createdAt),
            CreateProduct(120, "Товар 120", "product-120", ProductType.Sewing, category.Id, createdAt));
        await db.SaveChangesAsync();

        var pageSpec = new GetAdminProductsSpec(null, null, null, "createdAt", false, 2, 1);
        var pageQuery = SpecificationEvaluator.Default.GetQuery(db.Products.AsQueryable(), pageSpec);
        var items = await pageQuery.ToListAsync();

        Assert.Multiple(() =>
        {
            Assert.That(items.Select(x => x.Id), Is.EqualTo(new[] { 120 }));
            Assert.That(pageQuery.ToQueryString(), Does.Contain("ORDER BY").And.Contain("\"Id\""));
        });
    }

    [Test]
    public async Task List_projection_uses_split_queries_for_multiple_collections()
    {
        await using var db = CreateContext(throwOnMultipleCollectionIncludeWarning: true);
        var category = ProductCategory.Create(ProductCategoryId.Create(5), "Категорія", "Категория", "category");
        var product = CreateProduct(140, "Product", "product", ProductType.Sewing, category.Id);
        db.ProductCategories.Add(category);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var query = SpecificationEvaluator.Default.GetQuery(
            db.Products.AsQueryable(),
            new GetAdminProductsSpec(null, null, null, "name", false, 1, 10));

        var items = await query.ToListAsync();

        Assert.That(items.Select(x => x.Id), Does.Contain(140));
    }

    [Test]
    public async Task List_projection_command_count_is_independent_of_page_size()
    {
        var counter = new QueryCommandCounter();
        await using var db = CreateContext(commandInterceptor: counter);
        var category = ProductCategory.Create(ProductCategoryId.Create(6), "Категорія", "Категория", "category-two");
        db.ProductCategories.Add(category);
        db.Products.AddRange(
            CreateProduct(150, "Product 1", "product-1", ProductType.Sewing, category.Id),
            CreateProduct(160, "Product 2", "product-2", ProductType.Sewing, category.Id));
        await db.SaveChangesAsync();

        counter.Reset();
        var oneItemQuery = SpecificationEvaluator.Default.GetQuery(
            db.Products.AsQueryable(),
            new GetAdminProductsSpec(null, null, null, "name", false, 1, 1));
        await oneItemQuery.ToListAsync();
        var oneItemCommandCount = counter.ReaderCommandCount;

        counter.Reset();
        var twoItemQuery = SpecificationEvaluator.Default.GetQuery(
            db.Products.AsQueryable(),
            new GetAdminProductsSpec(null, null, null, "name", false, 1, 2));
        await twoItemQuery.ToListAsync();

        Assert.That(counter.ReaderCommandCount, Is.EqualTo(oneItemCommandCount));
    }

    [Test]
    public async Task Media_url_projection_reads_only_requested_uploaded_media_files_in_sql()
    {
        await using var db = CreateContext();
        var media = MediaFile.CreatePending(
            MediaFileId.Create(501),
            "product.jpg",
            "image/jpeg",
            1024,
            "test",
            "catalog",
            "products/501.jpg");
        media.MarkUploaded("https://cdn.example.test/501.jpg", null, null);
        db.MediaFiles.Add(media);
        await db.SaveChangesAsync();

        var query = SpecificationEvaluator.Default.GetQuery(
            db.MediaFiles.AsQueryable(),
            new ProductMediaUrlsByIdsSpec([501]));
        var result = await query.SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result.MediaFileId, Is.EqualTo(501));
            Assert.That(result.Url, Is.EqualTo("https://cdn.example.test/501.jpg"));
            Assert.That(query.ToQueryString(), Does.Contain("WHERE").And.Contain("MediaFiles"));
        });
    }

    private CatalogDbContext CreateContext(
        bool throwOnMultipleCollectionIncludeWarning = false,
        DbCommandInterceptor? commandInterceptor = null)
        => new(new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(_testConnectionString)
            .ConfigureWarnings(warnings =>
            {
                if (throwOnMultipleCollectionIncludeWarning)
                    warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning);
            })
            .AddInterceptors(commandInterceptor is null ? [] : [commandInterceptor])
            .Options);

    private sealed class QueryCommandCounter : DbCommandInterceptor
    {
        public int ReaderCommandCount { get; private set; }

        public void Reset() => ReaderCommandCount = 0;

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            ReaderCommandCount++;
            return ValueTask.FromResult(result);
        }
    }

    private static Product CreateProduct(
        int id,
        string name,
        string slug,
        ProductType type,
        ProductCategoryId categoryId,
        DateTimeOffset? createdAt = null)
    {
        var productId = ProductId.Create(id);
        var product = Product.Create(productId, name, name, ProductSlug.Create(slug), type, createdAt ?? DateTimeOffset.UtcNow);
        product.ReplaceCategories([categoryId], DateTimeOffset.UtcNow);
        return product;
    }
}
