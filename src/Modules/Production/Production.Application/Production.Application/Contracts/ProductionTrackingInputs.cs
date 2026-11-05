namespace Production.Application.Contracts;

/// <summary>The order version the change is made on; a stale one gets «Завдання вже змінили».</summary>
public sealed record ProductionOrderVersionInput(int Version);

/// <param name="Stages">Ticked етапи: New, FabricOrdered, FabricAvailable, Cutting, Application, Sewing, Ready, Shipped, Complete.</param>
public sealed record SetProductionItemStagesInput(int Version, IReadOnlyList<string> Stages);

public sealed record CreateProductionShipmentInput(int Version, int Quantity, DateOnly ShippedOn, string? Comment);

public sealed record CancelProductionOrderInput(int Version, string Reason);

/// <param name="TelegramChatId">The workshop group; sending needs it.</param>
/// <param name="TelegramThreadId">The topic in a group with topics; null posts to the main chat.</param>
public sealed record WorkshopSettingsInput(bool TelegramEnabled, long? TelegramChatId, int? TelegramThreadId);

/// <summary>Stages, file links and the Telegram mark of an order moved from the old «Цех».</summary>
/// <param name="LegacyId">The order's id in the old app, as in production-orders.json.</param>
public sealed record ProductionProgressImportRow(
    string LegacyId,
    IReadOnlyList<ProductionItemProgressImportRow> Items,
    IReadOnlyList<ProductionFileImportRow> Files,
    bool TelegramSent,
    DateTimeOffset? TelegramSentAt);

/// <param name="Position">The item's place in the order, from 1, as in production-orders.json.</param>
public sealed record ProductionItemProgressImportRow(int Position, IReadOnlyList<string> Stages);

/// <param name="Position">The item the file belongs to; null for the whole order.</param>
/// <param name="Kind">Sizes, Application, Extra or Photo.</param>
public sealed record ProductionFileImportRow(
    int? Position,
    string Kind,
    string Name,
    string ContentType,
    long Bytes,
    string Url,
    DateTimeOffset CreatedAt,
    string? CreatedByName);

/// <summary>The КП prices of an order moved from the old «Цех», as in production-report-prices.json.</summary>
/// <param name="LegacyId">The order's id in the old app, as in production-orders.json.</param>
public sealed record ProductionReportPriceImportRow(string LegacyId, IReadOnlyList<ProductionItemReportPriceImportRow> Items);

/// <param name="Position">The item's place in the order, from 1, as in production-orders.json.</param>
/// <param name="UnitPrice">UAH per item from the КП.</param>
public sealed record ProductionItemReportPriceImportRow(int Position, decimal UnitPrice);
