using Microsoft.EntityFrameworkCore;
using Npgsql;
using Scheduling.Infrastructure.Persistence;

namespace IntegrationTests.Platform.Jobs;

/// <summary>Two scheduler ticks at once (database audit 2026-11-15, 5.6): the second finds the lock taken.</summary>
[NonParallelizable]
public sealed class SchedulerTickLockTests
{
    [Test]
    public async Task A_second_tick_gets_no_lock_until_the_first_ends()
    {
        var connection = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection))
            Assert.Ignore("Set BREVIERP_CATALOG_TEST_CONNECTION to a PostgreSQL connection string.");

        await using var first = Context(connection);
        await using var second = Context(connection);

        var held = await first.TryLockTickAsync(CancellationToken.None);
        var refused = await second.TryLockTickAsync(CancellationToken.None);
        await held!.DisposeAsync();
        await using var after = await second.TryLockTickAsync(CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(held, Is.Not.Null);
            Assert.That(refused, Is.Null);
            Assert.That(after, Is.Not.Null, "the lock is released when the first tick ends");
        });
    }

    private static SchedulingDbContext Context(string connection) => new(new DbContextOptionsBuilder<SchedulingDbContext>()
        .UseNpgsql(new NpgsqlConnectionStringBuilder(connection).ConnectionString).Options);
}
