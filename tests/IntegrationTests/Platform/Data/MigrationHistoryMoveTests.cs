using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Reminders.Infrastructure.Persistence;

namespace IntegrationTests.Platform.Data;

/// <summary>
/// A module that moves its migration history from <c>public</c> to its own schema (database rules 5.1): the first
/// migrator run copies its rows there, applies nothing twice, and leaves the old rows for a rollback.
/// </summary>
[NonParallelizable]
public sealed class MigrationHistoryMoveTests
{
    [Test]
    public async Task The_migrator_copies_the_module_history_from_public_and_applies_nothing_again()
    {
        var baseConnection = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(baseConnection))
            Assert.Ignore("Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string.");
        var database = $"brevierp_history_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnection);
        var maintenance = new NpgsqlConnectionStringBuilder(builder.ConnectionString) { Database = "postgres" }.ConnectionString;
        builder.Database = database;
        await ExecuteAsync(maintenance, $"CREATE DATABASE \"{database}\"");
        try
        {
            // The previous release: history in public, plus a row of another module.
            await using (var before = new RemindersDbContext(new DbContextOptionsBuilder<RemindersDbContext>()
                             .UseNpgsql(builder.ConnectionString).Options))
            {
                await before.Database.MigrateAsync();
                await before.Database.ExecuteSqlRawAsync(
                    """INSERT INTO public."__EFMigrationsHistory" VALUES ('20200101000000_OtherModule', '10.0.0')""");
            }

            var services = new ServiceCollection()
                .AddDbContext<RemindersDbContext>(x => x.UseNpgsql(builder.ConnectionString,
                    o => o.MigrationsHistoryTable("__EFMigrationsHistory", RemindersDbContext.Schema)))
                .BuildServiceProvider();
            var migrator = new DbMigrator<RemindersDbContext>(services, NullLogger<DbMigrator<RemindersDbContext>>.Instance);

            await migrator.MigrateAsync();
            await migrator.MigrateAsync();

            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<RemindersDbContext>();
            var all = db.Database.GetMigrations().ToArray();
            Assert.Multiple(async () =>
            {
                Assert.That(await db.Database.GetAppliedMigrationsAsync(), Is.EqualTo(all));
                Assert.That(await CountAsync(builder.ConnectionString, $"\"{RemindersDbContext.Schema}\""), Is.EqualTo(all.Length));
                Assert.That(await CountAsync(builder.ConnectionString, "public"), Is.EqualTo(all.Length + 1));
            });
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(maintenance, $"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
        }
    }

    private static async Task<long> CountAsync(string connection, string schema)
    {
        await using var db = new NpgsqlConnection(connection);
        await db.OpenAsync();
        await using var command = new NpgsqlCommand($"SELECT count(*) FROM {schema}.\"__EFMigrationsHistory\"", db);
        return (long)(await command.ExecuteScalarAsync())!;
    }

    private static async Task ExecuteAsync(string connection, string sql)
    {
        await using var db = new NpgsqlConnection(connection);
        await db.OpenAsync();
        await using var command = new NpgsqlCommand(sql, db);
        await command.ExecuteNonQueryAsync();
    }
}
