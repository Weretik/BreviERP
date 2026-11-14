namespace Production.Application.Features.Staff.Commands.ImportWorkshopStaff;

/// <param name="Action">created, updated or unchanged.</param>
public sealed record WorkshopStaffImportLine(string FullName, string Position, string Action);
