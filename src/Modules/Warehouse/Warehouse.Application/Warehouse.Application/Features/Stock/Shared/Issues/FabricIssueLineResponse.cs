namespace Warehouse.Application.Features.Stock.Shared.Issues;

/// <param name="Purpose">Planned, Replacement or Additional.</param>
/// <param name="PlannedMeters">The plan of the products cut for its planned part (or the planned part, older issues).</param>
/// <param name="Layer">Layer of the planned part or role of an additional line; null when unknown.</param>
public sealed record FabricIssueLineResponse(
    int Id,
    string Purpose,
    string? PlanFabricCode,
    string? PlanColorKey,
    decimal? PlannedMeters,
    int StockId,
    string FabricCode,
    string FabricName,
    string Color,
    decimal Meters,
    decimal CoveredMeters,
    string? Layer,
    string? Note,
    bool IsVoided);
