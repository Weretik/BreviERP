namespace Production.Application.Features.Work.Commands.ImportProductionWork;

/// <param name="Action">created, unchanged or error.</param>
public sealed record ProductionWorkImportLine(
    string LegacyId, int? OrderNumber, int Position, string EmployeeName, string Kind, int Quantity, string Action,
    string? Error);
