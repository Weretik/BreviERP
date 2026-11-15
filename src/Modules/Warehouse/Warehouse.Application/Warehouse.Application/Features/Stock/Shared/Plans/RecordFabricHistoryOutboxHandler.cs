using BuildingBlocks.Application.Outbox;
using Warehouse.Application.Abstractions;

namespace Warehouse.Application.Features.Stock.Shared.Plans;

/// <summary>Writes a saved <see cref="FabricHistoryNote"/> to the order history; Production skips a line it has.</summary>
public sealed class RecordFabricHistoryOutboxHandler(IWarehouseProductionReader production) : IOutboxHandler
{
    public const string MessageType = "warehouse.fabric-history.record";

    public string Type => MessageType;

    public async Task<string?> HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var note = OutboxMessage.Read<FabricHistoryNote>(payload);
        await production.RecordFabricAsync(note.OrderId, note.ItemId, note.Text, note.ActorName, note.At,
            cancellationToken);
        return null;
    }
}
