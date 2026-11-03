using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests;

/// <summary>Shared daily sync job (--sync): selection is checked before any service or database is touched.</summary>
[TestFixture]
public sealed class SyncJobCommandTests
{
    [Test]
    public async Task Unknown_sync_task_fails_before_running_anything()
    {
        var services = new NoServices();
        var configuration = Configuration(new() { ["sync"] = "true", ["sync-tasks"] = "keepin-suppliers,unknown" });
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(services, configuration));
        Assert.That(exception!.Message, Is.EqualTo("Unknown sync task: unknown. Known: keepin-suppliers, keepin-clients, sewing-prices."));
        Assert.That(services.Calls, Is.Zero);
    }

    [Test]
    public async Task Task_selection_without_sync_mode_never_runs_migration_or_seed()
    {
        var services = new NoServices();
        var configuration = Configuration(new() { ["sync-tasks"] = "keepin-suppliers" });
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(services, configuration));
        Assert.That(exception!.Message, Is.EqualTo("Sync task selection requires the explicit sync mode."));
        Assert.That(services.Calls, Is.Zero);
    }

    [Test]
    public async Task Sync_cannot_be_combined_with_another_mode()
    {
        var services = new NoServices();
        var configuration = Configuration(new() { ["sync"] = "true", ["reminders-dispatch"] = "true" });
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Dispatch(services, configuration));
        Assert.That(exception!.Message, Is.EqualTo("Select exactly one explicit seed job mode."));
        Assert.That(services.Calls, Is.Zero);
    }

    private static IConfiguration Configuration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private static Task Dispatch(IServiceProvider services, IConfiguration configuration)
    {
        var method = Assembly.Load("Host.Seed").GetType("Host.Seed.Jobs.SeedJobDispatcher")!
            .GetMethod("RunAsync", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        try { return (Task)method.Invoke(null, [services, configuration])!; }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }

    private sealed class NoServices : IServiceProvider
    {
        internal int Calls { get; private set; }

        public object? GetService(Type serviceType)
        {
            Calls++;
            throw new InvalidOperationException("Unexpected service resolution.");
        }
    }
}

/// <summary>Runs the shared daily sync job (--sync) with the given steps, as Cloud Scheduler does.</summary>
internal static class SyncJob
{
    public static Task RunAsync(IServiceProvider services, string tasks)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["sync"] = "true", ["sync-tasks"] = tasks
        }).Build();
        var method = Assembly.Load("Host.Seed").GetType("Host.Seed.Jobs.SeedJobDispatcher")!
            .GetMethod("RunAsync", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
        try { return (Task)method.Invoke(null, [services, configuration])!; }
        catch (TargetInvocationException exception) when (exception.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException).Throw();
            throw;
        }
    }
}
