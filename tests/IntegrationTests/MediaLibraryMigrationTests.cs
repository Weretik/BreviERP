using Media.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using Catalog.Infrastructure.Persistence;

namespace IntegrationTests;

/// <summary>InitialMedia copies the uploaded Catalog photos with their ids into folder «Товари» and keeps the old table.</summary>
[NonParallelizable]
public sealed class MediaLibraryMigrationTests
{
    private const string CatalogBeforeMove = "20261103111437_AddSewingCompositionLineComments";
    private string _databaseName = null!;
    private string _testConnectionString = null!;
    private string _maintenanceConnectionString = null!;

    [SetUp]
    public async Task CreateTestDatabase()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        Assert.That(baseConnectionString, Is.Not.Null.And.Not.Empty,
            "Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string with CREATE DATABASE permission.");
        _databaseName = $"brevierp_media_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
        _maintenanceConnectionString = new NpgsqlConnectionStringBuilder(builder.ConnectionString) { Database = "postgres" }.ConnectionString;
        builder.Database = _databaseName;
        _testConnectionString = builder.ConnectionString;
        await ExecuteMaintenanceAsync($"CREATE DATABASE \"{_databaseName}\"");
    }

    [TearDown]
    public async Task DropTestDatabase()
    {
        NpgsqlConnection.ClearAllPools();
        await ExecuteMaintenanceAsync($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)");
    }

    [Test]
    public async Task Uploaded_catalog_photos_move_with_their_ids_and_new_ids_continue_after_them()
    {
        await using var catalog = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>().UseNpgsql(_testConnectionString).Options);
        await catalog.Database.GetService<IMigrator>().MigrateAsync(CatalogBeforeMove);
        await catalog.Database.ExecuteSqlRawAsync("""
            INSERT INTO catalog."MediaFiles" ("Id", "FileName", "ContentType", "SizeInBytes", "StorageProvider", "BucketName", "StorageKey", "PublicUrl", "Status")
            VALUES (41, 'coat.jpg', 'image/jpeg', 10, 'adm.tools', 'products', '/products/original/a.jpg', 'https://cdn.test/a.jpg', 2),
                   (42, 'broken.png', 'image/png', 10, 'adm.tools', 'products', '/products/original/b.png', NULL, 1),
                   (43, 'gloves.webp', 'image/webp', 10, 'adm.tools', 'products', '/products/original/c.webp', 'https://cdn.test/c.webp', 2)
            """);
        await catalog.Database.MigrateAsync();

        await using var media = new MediaDbContext(new DbContextOptionsBuilder<MediaDbContext>()
            .UseNpgsql(_testConnectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", MediaDbContext.Schema)).Options);
        await media.Database.MigrateAsync();

        var folder = await media.Folders.SingleAsync();
        var files = await media.Files.OrderBy(x => x.Id).ToListAsync();
        var nextId = await media.Database
            .SqlQueryRaw<int>("SELECT nextval(pg_get_serial_sequence('media.\"Files\"', 'Id'))::int AS \"Value\"").SingleAsync();
        var keptCatalogRows = await catalog.Database
            .SqlQueryRaw<int>("SELECT count(*)::int AS \"Value\" FROM catalog.\"MediaFiles\"").SingleAsync();

        Assert.Multiple(() =>
        {
            Assert.That(folder.Name, Is.EqualTo("Товари"));
            Assert.That(files.Select(x => x.Id), Is.EqualTo(new[] { 41, 43 }));
            Assert.That(files.Select(x => x.FolderId), Is.All.EqualTo(folder.Id));
            Assert.That(files[0].PublicUrl, Is.EqualTo("https://cdn.test/a.jpg"));
            Assert.That(nextId, Is.EqualTo(44));
            Assert.That(keptCatalogRows, Is.EqualTo(3));
        });
    }

    [Test]
    public async Task New_database_without_catalog_gets_an_empty_library()
    {
        await using var media = new MediaDbContext(new DbContextOptionsBuilder<MediaDbContext>()
            .UseNpgsql(_testConnectionString, x => x.MigrationsHistoryTable("__EFMigrationsHistory", MediaDbContext.Schema)).Options);
        await media.Database.MigrateAsync();

        Assert.Multiple(async () =>
        {
            Assert.That(await media.Folders.CountAsync(), Is.Zero);
            Assert.That(await media.Files.CountAsync(), Is.Zero);
        });
    }

    private async Task ExecuteMaintenanceAsync(string sql)
    {
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}
