namespace Production.Contracts.FabricPlans;

/// <summary>An order item and its fabric plan by colour, for «Склад тканин».</summary>
/// <param name="IsOpen">The order is «Нове» and not deleted.</param>
public sealed record OrderPlanItem(
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
    IReadOnlyList<OrderPlanPart> Parts,
    string? ModelCode,
    DateTimeOffset OrderCreatedAt,
    DateOnly? DueDate);

/// <summary>Metres of one fabric in one colour that an item plans.</summary>
public sealed record OrderPlanPart(string FabricCode, string Color, string ColorKey, decimal Meters);

/// <summary>An order moved from the old «Цех», by its old id.</summary>
public sealed record LegacyOrder(string LegacyId, int OrderId, int Number, IReadOnlyList<LegacyOrderItem> Items);

public sealed record LegacyOrderItem(int Position, int ItemId, string ModelName, string? ModelCode, int Quantity);

/// <summary>What an item of an open order takes besides fabric; <paramref name="Lines"/> is null without composition.</summary>
public sealed record AssemblyItem(
    int OrderId,
    int OrderNumber,
    string ClientName,
    DateTimeOffset OrderCreatedAt,
    int ItemId,
    string ModelName,
    string? ModelCode,
    int Quantity,
    IReadOnlyList<AssemblyLine>? Lines,
    DateOnly? DueDate);

/// <param name="Kind">Accessory or OtherCost.</param>
/// <param name="Unit">Metre, Piece, Service or Kilogram.</param>
public sealed record AssemblyLine(string Kind, string? Code, string? Name, decimal Quantity, string Unit,
    string? Comment);

/// <summary>
/// What «Склад тканин» needs of the workshop: the plan of items by colour from the norms (PRD-D28), what items take
/// besides fabric, the orders moved from the old «Цех», and the line «Тканина» in an item's history.
/// </summary>
public interface IOrderFabricPlans
{
    /// <summary>Items of «Нове» orders; <paramref name="fabricCode"/> narrows to the items that plan that fabric.</summary>
    Task<IReadOnlyList<OrderPlanItem>> OpenItemsAsync(string? fabricCode, CancellationToken cancellationToken);

    /// <summary>One item of an order that is not deleted, whatever its status.</summary>
    Task<OrderPlanItem?> ItemAsync(int orderId, int itemId, CancellationToken cancellationToken);

    /// <summary>The given items of orders that are not deleted; unknown ids are left out.</summary>
    Task<IReadOnlyList<OrderPlanItem>> ItemsAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken);

    Task<IReadOnlyList<AssemblyItem>> OpenAssembliesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<LegacyOrder>> LegacyOrdersAsync(IReadOnlyCollection<string> legacyIds,
        CancellationToken cancellationToken);

    /// <summary>Adds «Тканина» to the history of the item; nothing when the order is gone.</summary>
    Task RecordFabricAsync(int orderId, int itemId, string text, string actorName, CancellationToken cancellationToken);
}
