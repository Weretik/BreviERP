namespace Production.Application.Features.Orders.Commands.ImportProductionProgress;

/// <summary>Stages, file links and the Telegram mark of an order moved from the old «Цех».</summary>
/// <param name="LegacyId">The order's id in the old app, as in production-orders.json.</param>
public sealed record ProductionProgressImportRow(
    string LegacyId,
    IReadOnlyList<ProductionItemProgressImportRow> Items,
    IReadOnlyList<ProductionFileImportRow> Files,
    bool TelegramSent,
    DateTimeOffset? TelegramSentAt);
