namespace Production.Application.Features.Staff.Commands.ImportWorkshopStaff;

/// <summary>One worker of the transfer file; matched to an existing employee by name, ignoring case.</summary>
public sealed record WorkshopEmployeeImportRow(string FullName, string Position);
