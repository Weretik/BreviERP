using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain;
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
            var name = ProductionText.Clean(row.FullName, WorkshopEmployee.FullNameMaxLength);
            var position = ProductionText.Clean(row.Position, WorkshopEmployee.PositionMaxLength);
            if (!active.TryGetValue(ProductionText.Key(name), out var employee))
            {
                if (!command.DryRun) db.Employees.Add(new WorkshopEmployee(name, position, command.UserId, now));
                lines.Add(new(name, position, "created"));
            }
            else if (employee.Position != position)
            {
                if (!command.DryRun) employee.Update(employee.FullName, position, command.UserId, now);
                lines.Add(new(employee.FullName, position, "updated"));
            }
            else lines.Add(new(employee.FullName, position, "unchanged"));
        }

        if (!command.DryRun) await db.SaveChangesAsync(cancellationToken);
        return Result.Success(new WorkshopStaffImportResult(command.DryRun, lines.Count(x => x.Action == "created"),
            lines.Count(x => x.Action == "updated"), lines.Count(x => x.Action == "unchanged"), lines));
    }
}
