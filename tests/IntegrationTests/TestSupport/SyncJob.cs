using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.TestSupport;

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
