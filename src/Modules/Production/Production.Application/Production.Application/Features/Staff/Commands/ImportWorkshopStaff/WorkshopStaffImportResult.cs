namespace Production.Application.Features.Staff.Commands.ImportWorkshopStaff;

public sealed record WorkshopStaffImportResult(
    bool DryRun, int Created, int Updated, int Unchanged, IReadOnlyList<WorkshopStaffImportLine> Lines);
