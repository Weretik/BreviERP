using BuildingBlocks.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Production.Application.Orders.Telegram;

namespace UnitTests.Production;

/// <summary>Runs the workshop's outbox messages right away, as the dispatcher does after a save.</summary>
internal sealed class InlineProductionOutbox(ProductionTestHarness workshop) : IOutboxDispatcher
{
    public async Task DispatchAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        var messages = await workshop.Db.OutboxMessages
            .Where(x => ids.Contains(x.Id) && x.ProcessedAt == null).ToListAsync(cancellationToken);
        foreach (var message in messages)
        {
            var error = await new SendProductionOrderTelegramOutboxHandler(workshop.Db, workshop.Telegram, workshop.Links,
                    workshop.Time, NullLogger<SendProductionOrderTelegramOutboxHandler>.Instance)
                .HandleAsync(message.Payload, cancellationToken);
            if (error is null) message.Succeed(workshop.Time.GetUtcNow());
            else message.Fail(error, workshop.Time.GetUtcNow());
        }

        await workshop.Db.SaveChangesAsync(cancellationToken);
    }

    public async Task<OutboxRun> DispatchDueAsync(CancellationToken cancellationToken)
    {
        var ids = await workshop.Db.OutboxMessages.Where(x => x.ProcessedAt == null).Select(x => x.Id)
            .ToListAsync(cancellationToken);
        await DispatchAsync(ids, cancellationToken);
        return new OutboxRun(ids.Count, 0);
    }
}
