namespace Production.Application.Features.Work.Commands.ImportProductionWork;

/// <summary>A record of the old «Цех» work log.</summary>
/// <param name="OrderLegacyId">The old order id, as in production-orders.json.</param>
/// <param name="Position">The item's position in that order.</param>
public sealed record ProductionWorkImportRow(
    string LegacyId,
    string OrderLegacyId,
    int Position,
    string EmployeeName,
    string Kind,
    int Quantity,
    bool OverPlan,
    string? Reason,
    string? Comment,
    decimal Rate,
    DateOnly WorkDate,
    DateTimeOffset At,
    string? ActorEmail,
    DateTimeOffset? VoidedAt,
    string? VoidReason);
