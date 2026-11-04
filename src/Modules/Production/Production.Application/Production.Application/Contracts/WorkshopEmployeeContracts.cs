namespace Production.Application.Contracts;

public sealed record WorkshopEmployeeInput(string FullName, string Position);

public sealed record WorkshopEmployeeResponse(
    int Id, string FullName, string Position, bool IsActive, DateTimeOffset UpdatedAt);

/// <summary>One worker of the transfer file; matched to an existing employee by name, ignoring case.</summary>
public sealed record WorkshopEmployeeImportRow(string FullName, string Position);

public sealed record WorkshopStaffImportResult(
    bool DryRun, int Created, int Updated, int Unchanged, IReadOnlyList<WorkshopStaffImportLine> Lines);

/// <param name="Action">created, updated or unchanged.</param>
public sealed record WorkshopStaffImportLine(string FullName, string Position, string Action);
