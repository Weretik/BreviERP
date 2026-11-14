using BuildingBlocks.Application.Outbox;
using Scheduling.Application.Abstractions;
using Scheduling.Application.Features.Catalog.Shared;

namespace Scheduling.Infrastructure.Integrations.Outbox;

/// <summary>The outbox of every module, run by each tick (SCH-001 R-04 a, PLT-003 R-03).</summary>
internal sealed class OutboxTickWork(IOutboxDispatcher dispatcher) : ITickWork
{
    public string TaskKey => ScheduledTaskKeys.Outbox;

    public async Task<string> RunAsync(CancellationToken cancellationToken)
    {
        var run = await dispatcher.DispatchDueAsync(cancellationToken);
        if (run.Failed > 0) throw new InvalidOperationException($"Не пішло повідомлень: {run.Failed} (надіслано {run.Processed}).");
        return run.Processed == 0 ? "Нічого досилати." : $"Надіслано повідомлень: {run.Processed}.";
    }
}
