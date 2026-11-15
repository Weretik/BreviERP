using BuildingBlocks.Application.Outbox;
using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Features.Stock.Shared.Plans;

/// <summary>A line «Тканина» for the order history, as the outbox carries it.</summary>
public sealed record FabricHistoryNote(int OrderId, int ItemId, string Text, string ActorName, DateTimeOffset At);

/// <summary>
/// «Тканина» lines in the order history (database audit 2026-11-15, 4.1). The note is saved in the warehouse outbox in
/// the same <c>SaveChanges</c> as the stock change, then Production writes it: a failure in between no longer loses
/// the line while the fabric is spent; the next dispatch writes it.
/// </summary>
public sealed class FabricHistory(IWarehouseDbContext db, IOutboxDispatcher outbox)
{
    /// <summary>Adds the note to the pending save; returns the message id for <see cref="SendAsync"/>.</summary>
    public Guid Note(int orderId, int itemId, string text, string actorName, DateTimeOffset now)
    {
        var message = OutboxMessage.Create(RecordFabricHistoryOutboxHandler.MessageType,
            new FabricHistoryNote(orderId, itemId, text, actorName, now), now);
        db.OutboxMessages.Add(message);
        return message.Id;
    }

    /// <summary>The try right after the save; it never throws, a failure waits for the next dispatch.</summary>
    public Task SendAsync(Guid id) => outbox.DispatchAsync([id], CancellationToken.None);
}
