namespace Production.Application.Features.Orders.Commands.ImportProductionProgress.Rows;

/// <param name="Action">updated, unchanged or error.</param>
public sealed record ProductionProgressImportLine(
    int? Number, string LegacyId, int Stages, int Files, string Action, string? Error);
