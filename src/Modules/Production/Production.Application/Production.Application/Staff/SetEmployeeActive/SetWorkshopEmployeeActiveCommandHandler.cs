using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Staff.SetEmployeeActive;

public sealed class SetWorkshopEmployeeActiveCommandHandler(IProductionDbContext db, TimeProvider clock)
    : ICommandHandler<SetWorkshopEmployeeActiveCommand, Result<WorkshopEmployeeResponse>>
{
    public async ValueTask<Result<WorkshopEmployeeResponse>> Handle(SetWorkshopEmployeeActiveCommand command,
        CancellationToken cancellationToken)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (employee is null) return Result<WorkshopEmployeeResponse>.NotFound(StaffMessages.NotFound);
        if (command.IsActive && !employee.IsActive
            && await EmployeeLookup.ActiveNameTakenAsync(db, employee.FullName, employee.Id, cancellationToken))
            return Result<WorkshopEmployeeResponse>.Conflict(StaffMessages.DuplicateName);

        employee.SetActive(command.IsActive, command.UserId, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(employee.ToResponse());
    }
}
