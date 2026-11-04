using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain;
using Production.Domain.Staff;

namespace Production.Application.Staff.ImportStaff;

/// <summary>
/// Transfer of workers from the old «Цех»: dryRun shows the preview; otherwise new names are added and positions of
/// known ones updated. Nobody is archived or deleted.
/// </summary>
public sealed record ImportWorkshopStaffCommand(IReadOnlyList<WorkshopEmployeeImportRow> Rows, bool DryRun, int? UserId)
    : ICommand<Result<WorkshopStaffImportResult>>;
