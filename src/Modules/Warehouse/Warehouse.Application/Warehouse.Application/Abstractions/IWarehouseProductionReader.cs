using Warehouse.Domain.Stock;

namespace Warehouse.Application.Abstractions;

/// <summary>
/// «Швейний цех» (Production) as the stock sees it: order items with their fabric plan by colour, the orders moved
/// from the old «Цех», and the item history. Production owns orders and norms; implemented in the host.
/// </summary>
public interface IWarehouseProductionReader
{
    /// <summary>Items of «Нове» orders; <paramref name="fabricCode"/> narrows to the items that plan that fabric.</summary>
    Task<IReadOnlyList<WarehousePlanItem>> OpenItemsAsync(string? fabricCode, CancellationToken cancellationToken);

    /// <summary>One item of an order that is not deleted, whatever its status; null when there is none.</summary>
    Task<WarehousePlanItem?> ItemAsync(int orderId, int itemId, CancellationToken cancellationToken);

    /// <summary>The given items of orders that are not deleted, whatever their status; unknown ids are left out.</summary>
    Task<IReadOnlyList<WarehousePlanItem>> ItemsAsync(IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken);

    /// <summary>Items of «Нове» orders with the non-fabric composition lines of their model.</summary>
    Task<IReadOnlyList<WarehouseAssemblyItem>> OpenAssembliesAsync(CancellationToken cancellationToken);

    /// <summary>Orders moved from the old «Цех», by their old id.</summary>
    Task<IReadOnlyList<WarehouseLegacyOrder>> LegacyOrdersAsync(IReadOnlyCollection<string> legacyIds,
        CancellationToken cancellationToken);

    /// <summary>Adds a line «Тканина» to the history of the item.</summary>
    Task RecordFabricAsync(int orderId, int itemId, string text, string actorName, CancellationToken cancellationToken);
}

/// <summary>A planned colour of an item: fabric, colour as typed, key, metres for the whole quantity, layer.</summary>
public sealed record WarehousePlanPart(string FabricCode, string Color, string ColorKey, decimal Meters,
    FabricLayer Layer = FabricLayer.Upper)
{
    public FabricPlanKey Key => new(FabricCode, ColorKey);
}

/// <summary>An order item and its plan by colour.</summary>
/// <param name="OrderVersion">Version of the order; an issue sent against another version is stale.</param>
/// <param name="IsOpen">The order is «Нове» and not deleted: its plan holds the stock and fabric may be issued.</param>
/// <param name="HasNorm">The model has a norm in «Норми витрати тканини».</param>
/// <param name="OrderCreatedAt">When the order was created: the queue of «Бронь» goes by it.</param>
public sealed record WarehousePlanItem(
    int OrderId,
    int OrderNumber,
    int OrderVersion,
    bool IsOpen,
    string ClientName,
    int ItemId,
    string ModelName,
    string? FabricCode,
    string? FabricName,
    int Quantity,
    bool HasNorm,
    IReadOnlyList<WarehousePlanPart> Parts,
    string? ModelCode = null,
    DateTimeOffset OrderCreatedAt = default,
    DateOnly? DueDate = null)
{
    public FabricMovementTarget Target => new(OrderId, ItemId, OrderNumber, ModelName);
}

public sealed record WarehouseLegacyOrder(string LegacyId, int OrderId, int Number,
    IReadOnlyList<WarehouseLegacyItem> Items);

public sealed record WarehouseLegacyItem(int Position, int ItemId, string ModelName, string? ModelCode = null,
    int Quantity = 0);

/// <summary>An item of an open order and what its model takes besides fabric.</summary>
/// <param name="Lines">Null when the model has no composition.</param>
public sealed record WarehouseAssemblyItem(
    int OrderId,
    int OrderNumber,
    string ClientName,
    DateTimeOffset OrderCreatedAt,
    int ItemId,
    string ModelName,
    string? ModelCode,
    int Quantity,
    IReadOnlyList<WarehouseAssemblyLine>? Lines,
    DateOnly? DueDate = null);

/// <param name="Kind">Accessory or OtherCost.</param>
/// <param name="Code">FUR-… or a cost code; null when unknown.</param>
/// <param name="Name">Reference name; null when the reference line is gone.</param>
/// <param name="Norm">Quantity per product.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record WarehouseAssemblyLine(string Kind, string? Code, string? Name, decimal Norm, string Unit,
    string? Comment);
