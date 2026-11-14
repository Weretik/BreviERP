using System.Collections.Concurrent;
using BuildingBlocks.Application.Outbox;
using BuildingBlocks.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace IntegrationTests.Platform.Outbox;

/// <summary>PLT-003 SC-02, SC-03, SC-04, SC-08 on PostgreSQL: same transaction, claimed once, retried, given up.</summary>
[NonParallelizable]
public sealed class OutboxDispatcherTests
{
    private string _databaseName = null!;
    private string _connectionString = null!;
    private string _maintenanceConnectionString = null!;
    private ServiceProvider _services = null!;

    [OneTimeSetUp]
    public async Task CreateDatabase()
    {
        var baseConnectionString = Environment.GetEnvironmentVariable("BREVIERP_CATALOG_TEST_CONNECTION");
        if (string.IsNullOrWhiteSpace(baseConnectionString))
            Assert.Ignore("Set BREVIERP_CATALOG_TEST_CONNECTION to run PostgreSQL tests.");

        _databaseName = $"brevierp_outbox_{Guid.NewGuid():N}";
        var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
        _maintenanceConnectionString = new NpgsqlConnectionStringBuilder(builder.ConnectionString) { Database = "postgres" }
            .ConnectionString;
        builder.Database = _databaseName;
        _connectionString = builder.ConnectionString;
        await using (var connection = new NpgsqlConnection(_maintenanceConnectionString))
        {
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE DATABASE \"{_databaseName}\"", connection);
            await command.ExecuteNonQueryAsync();
        }

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestOutboxContext>(x => x.UseNpgsql(_connectionString));
        services.AddOutbox<TestOutboxContext>();
        services.AddOutboxHandler<RecordingHandler>();
        services.AddOutboxHandler<FailingHandler>();
        _services = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        await using var scope = _services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TestOutboxContext>().Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task DropDatabase()
    {
        if (_services is not null) await _services.DisposeAsync();
        if (string.IsNullOrWhiteSpace(_maintenanceConnectionString)) return;
        NpgsqlConnection.ClearAllPools();
        await using var connection = new NpgsqlConnection(_maintenanceConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{_databaseName}\" WITH (FORCE)", connection);
        await command.ExecuteNonQueryAsync();
    }

    [SetUp]
    public async Task Clear()
    {
        RecordingHandler.Calls.Clear();
        RecordingHandler.Delay = TimeSpan.Zero;
        await using var scope = _services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<TestOutboxContext>().Database
            .ExecuteSqlRawAsync("""DELETE FROM outbox_test."OutboxMessages" """);
    }

    [Test]
    public async Task A_rolled_back_change_leaves_no_message()
    {
        await using (var scope = _services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<TestOutboxContext>();
            await using var transaction = await db.Database.BeginTransactionAsync();
            db.Add(OutboxMessage.Create(RecordingHandler.MessageType, new Payload("a"), DateTimeOffset.UtcNow));
            await db.SaveChangesAsync();
            await transaction.RollbackAsync();
        }

        Assert.That(await CountAsync(), Is.Zero);
    }

    [Test]
    public async Task The_immediate_try_runs_the_message_once()
    {
        var id = await SaveAsync(RecordingHandler.MessageType, "once");

        await DispatchAsync(d => d.DispatchAsync([id], CancellationToken.None));
        await DispatchAsync(d => d.DispatchAsync([id], CancellationToken.None));
        var run = await DispatchAsync(d => d.DispatchDueAsync(CancellationToken.None));

        var message = await LoadAsync(id);
        Assert.Multiple(() =>
        {
            Assert.That(RecordingHandler.Calls, Is.EqualTo(new[] { "once" }));
            Assert.That(message.ProcessedAt, Is.Not.Null);
            Assert.That(run, Is.EqualTo(OutboxRun.Empty));
        });
    }

    [Test]
    public async Task Two_dispatches_at_the_same_time_take_a_message_once()
    {
        RecordingHandler.Delay = TimeSpan.FromMilliseconds(300);
        await SaveAsync(RecordingHandler.MessageType, "parallel");

        var runs = await Task.WhenAll(
            DispatchAsync(d => d.DispatchDueAsync(CancellationToken.None)),
            DispatchAsync(d => d.DispatchDueAsync(CancellationToken.None)));

        Assert.Multiple(() =>
        {
            Assert.That(RecordingHandler.Calls, Is.EqualTo(new[] { "parallel" }));
            Assert.That(runs.Sum(x => x.Processed), Is.EqualTo(1));
        });
    }

    [Test]
    public async Task A_failure_is_kept_and_waits_for_the_next_try()
    {
        var id = await SaveAsync(FailingHandler.MessageType, "x");

        var first = await DispatchAsync(d => d.DispatchDueAsync(CancellationToken.None));
        var second = await DispatchAsync(d => d.DispatchDueAsync(CancellationToken.None));

        var message = await LoadAsync(id);
        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(new OutboxRun(0, 1)));
            Assert.That(second, Is.EqualTo(OutboxRun.Empty), "not due again for a minute");
            Assert.That(message.Attempts, Is.EqualTo(1));
            Assert.That(message.LastError, Is.EqualTo(FailingHandler.Reason));
            Assert.That(message.NextAttemptAt, Is.GreaterThan(DateTimeOffset.UtcNow.AddSeconds(30)));
        });
    }

    [Test]
    public async Task The_tenth_failure_gives_up_and_the_message_is_not_taken_again()
    {
        var id = await SaveAsync(FailingHandler.MessageType, "x");
        await using (var scope = _services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<TestOutboxContext>().Database.ExecuteSqlRawAsync(
                """UPDATE outbox_test."OutboxMessages" SET "Attempts" = 9""");

        await DispatchAsync(d => d.DispatchDueAsync(CancellationToken.None));
        await using (var scope = _services.CreateAsyncScope())
            await scope.ServiceProvider.GetRequiredService<TestOutboxContext>().Database.ExecuteSqlRawAsync(
                """UPDATE outbox_test."OutboxMessages" SET "NextAttemptAt" = now() - interval '1 minute'""");
        var after = await DispatchAsync(d => d.DispatchDueAsync(CancellationToken.None));

        var message = await LoadAsync(id);
        Assert.Multiple(() =>
        {
            Assert.That(message.Attempts, Is.EqualTo(OutboxMessage.MaxAttempts));
            Assert.That(message.ProcessedAt, Is.Null);
            Assert.That(after, Is.EqualTo(OutboxRun.Empty));
        });
    }

    [Test]
    public async Task A_message_without_a_handler_fails_with_the_reason()
    {
        var id = await SaveAsync("nobody.handles", "x");

        await DispatchAsync(d => d.DispatchAsync([id], CancellationToken.None));

        Assert.That((await LoadAsync(id)).LastError, Is.EqualTo("No handler for nobody.handles."));
    }

    private async Task<Guid> SaveAsync(string type, string text)
    {
        await using var scope = _services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TestOutboxContext>();
        var message = OutboxMessage.Create(type, new Payload(text), DateTimeOffset.UtcNow.AddSeconds(-1));
        db.Add(message);
        await db.SaveChangesAsync();
        return message.Id;
    }

    private async Task<T> DispatchAsync<T>(Func<IOutboxDispatcher, Task<T>> run)
    {
        await using var scope = _services.CreateAsyncScope();
        return await run(scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>());
    }

    private async Task DispatchAsync(Func<IOutboxDispatcher, Task> run)
    {
        await using var scope = _services.CreateAsyncScope();
        await run(scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>());
    }

    private async Task<OutboxMessage> LoadAsync(Guid id)
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<TestOutboxContext>().Set<OutboxMessage>().AsNoTracking()
            .SingleAsync(x => x.Id == id);
    }

    private async Task<int> CountAsync()
    {
        await using var scope = _services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<TestOutboxContext>().Set<OutboxMessage>().CountAsync();
    }

    public sealed record Payload(string Text);

    public sealed class TestOutboxContext(DbContextOptions<TestOutboxContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("outbox_test");
            modelBuilder.AddOutbox();
        }
    }

    private sealed class RecordingHandler : IOutboxHandler
    {
        public const string MessageType = "test.record";
        public static readonly ConcurrentQueue<string> Calls = new();
        public static TimeSpan Delay { get; set; }

        public string Type => MessageType;

        public async Task<string?> HandleAsync(string payload, CancellationToken cancellationToken)
        {
            await Task.Delay(Delay, cancellationToken);
            Calls.Enqueue(OutboxMessage.Read<Payload>(payload).Text);
            return null;
        }
    }

    private sealed class FailingHandler : IOutboxHandler
    {
        public const string MessageType = "test.fail";
        public const string Reason = "Telegram недоступний.";

        public string Type => MessageType;

        public Task<string?> HandleAsync(string payload, CancellationToken cancellationToken) =>
            Task.FromResult<string?>(Reason);
    }
}
