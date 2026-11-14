using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.Staff;

namespace Production.Application.Staff.ImportStaff;

public sealed class ImportWorkshopStaffCommandHandler(IProductionDbContext db, TimeProvider clock)
    : ICommandHandler<ImportWorkshopStaffCommand, Result<WorkshopStaffImportResult>>
{
    public async ValueTask<Result<WorkshopStaffImportResult>> Handle(ImportWorkshopStaffCommand command,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var active = (await db.Employees.Where(x => x.IsActive).ToListAsync(cancellationToken))
            .ToDictionary(x => x.NameKey);
        var lines = new List<WorkshopStaffImportLine>(command.Rows.Count);
        foreach (var row in command.Rows)
        {
            var checkedRow = WorkshopEmployee.Create(row.FullName, row.Position, command.UserId, now);
            if (checkedRow.IsFailure) return checkedRow.Error!.ToResult<WorkshopStaffImportResult>();
            var created = checkedRow.Value;
            if (!active.TryGetValue(created.NameKey, out var employee))
            {
                if (!command.DryRun) db.Employees.Add(created);
                lines.Add(new(created.FullName, created.Position, "created"));
            }
            else if (employee.Position != created.Position)
            {
                if (!command.DryRun
                    && employee.Update(employee.FullName, created.Position, command.UserId, now) is { IsFailure: true } bad)
                    return bad.Error!.ToResult<WorkshopStaffImportResult>();
                lines.Add(new(employee.FullName, created.Position, "updated"));
            }
            else lines.Add(new(employee.FullName, created.Position, "unchanged"));
        }

        if (!command.DryRun) await db.SaveChangesAsync(cancellationToken);
        return Result.Success(new WorkshopStaffImportResult(command.DryRun, lines.Count(x => x.Action == "created"),
            lines.Count(x => x.Action == "updated"), lines.Count(x => x.Action == "unchanged"), lines));
    }
}
