using BuildingBlocks.Application.Outbox;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Infrastructure.Outbox;

internal sealed class OutboxDispatcher(IEnumerable<IOutboxModule> modules, ILogger<OutboxDispatcher> logger)
    : IOutboxDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0) return;
        foreach (var module in modules)
        {
            try
            {
                await module.RunAsync(ids, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // The rows stay due; the scheduled dispatch takes them.
                logger.LogWarning(exception, "Immediate outbox try failed");
            }
        }
    }

    public async Task<OutboxRun> DispatchDueAsync(CancellationToken cancellationToken)
    {
        var run = OutboxRun.Empty;
        foreach (var module in modules)
            run = run.Add(await module.RunAsync(null, cancellationToken));
        return run;
    }
}
