using System.Diagnostics;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Reminders.Infrastructure.Persistence;

namespace IntegrationTests.Platform.Data;

/// <summary>A migration that meets a busy table fails fast (database rules 5.3) instead of waiting behind it.</summary>
[NonParallelizable]
public sealed class MigrationLockTimeoutTests
{
    [Test]
    public async Task The_migrator_gives_up_on_a_locked_table_after_the_lock_timeout()
    {
        var baseConnection = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(baseConnection))
            Assert.Ignore("Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string.");
        var database = $"brevierp_lock_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnection);
        var maintenance = new NpgsqlConnectionStringBuilder(builder.ConnectionString) { Database = "postgres" }.ConnectionString;
        builder.Database = database;
        await ExecuteAsync(maintenance, $"CREATE DATABASE \"{database}\"");
        try
        {
            var services = new ServiceCollection()
                .AddDbContext<RemindersDbContext>(x => x.UseNpgsql(builder.ConnectionString,
                    o => o.MigrationsHistoryTable("__EFMigrationsHistory", "reminders")))
                .BuildServiceProvider();
            await using (var scope = services.CreateAsyncScope())
                await scope.ServiceProvider.GetRequiredService<RemindersDbContext>().GetService<IMigrator>()
                    .MigrateAsync(BeforeTheLastMigration);
            var migrator = new DbMigrator<RemindersDbContext>(services, NullLogger<DbMigrator<RemindersDbContext>>.Instance);

            await using var holder = new NpgsqlConnection(builder.ConnectionString);
            await holder.OpenAsync();
            await using var transaction = await holder.BeginTransactionAsync();
            await using (var lockHistory = new NpgsqlCommand(
                             """LOCK TABLE reminders."Occurrences" IN ACCESS EXCLUSIVE MODE""", holder, transaction))
                await lockHistory.ExecuteNonQueryAsync();

            var watch = Stopwatch.StartNew();
            Exception? error = null;
            try
            {
                await migrator.MigrateAsync();
            }
            catch (Exception exception)
            {
                error = exception;
            }

            Assert.Multiple(() =>
            {
                Assert.That(error, Is.Not.Null);
                Assert.That(error?.GetBaseException(), Is.InstanceOf<PostgresException>().With.Property(nameof(PostgresException.SqlState))
                    .EqualTo("55P03"), "lock_not_available");
                Assert.That(watch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(30)));
            });
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteAsync(maintenance, $"DROP DATABASE IF EXISTS \"{database}\" WITH (FORCE)");
        }
    }

    /// <summary>The next migration adds a column to the occurrences, so it needs their table.</summary>
    private const string BeforeTheLastMigration = "20261111203312_AddChatSiteRequests";

    private static async Task ExecuteAsync(string connection, string sql)
    {
        await using var open = new NpgsqlConnection(connection);
        await open.OpenAsync();
        await using var command = new NpgsqlCommand(sql, open);
        await command.ExecuteNonQueryAsync();
    }
}
