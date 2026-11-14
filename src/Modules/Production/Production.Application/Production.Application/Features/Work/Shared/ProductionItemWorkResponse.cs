namespace Production.Application.Features.Work.Shared;

/// <summary>«Робота» of an item: how much is sewn and cut of the plan, and every record.</summary>
/// <param name="CannotRecord">Why work cannot be recorded now, or null when it can.</param>
public sealed record ProductionItemWorkResponse(
    int OrderId,
    int ItemId,
    int OrderNumber,
    string ModelName,
    int Quantity,
    decimal? SewingRate,
    int Sewn,
    int Cut,
    IReadOnlyList<ProductionWorkResponse> Rows,
    bool CanRecord,
    string? CannotRecord);
