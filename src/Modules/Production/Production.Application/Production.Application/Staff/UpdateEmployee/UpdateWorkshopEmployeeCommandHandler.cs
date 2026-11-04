using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.Staff.UpdateEmployee;

public sealed class UpdateWorkshopEmployeeCommandHandler(IProductionDbContext db, TimeProvider clock)
    : ICommandHandler<UpdateWorkshopEmployeeCommand, Result<WorkshopEmployeeResponse>>
{
    public async ValueTask<Result<WorkshopEmployeeResponse>> Handle(UpdateWorkshopEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var employee = await db.Employees.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (employee is null) return Result<WorkshopEmployeeResponse>.NotFound(StaffMessages.NotFound);
        if (employee.IsActive
            && await EmployeeLookup.ActiveNameTakenAsync(db, command.Input.FullName, employee.Id, cancellationToken))
            return Result<WorkshopEmployeeResponse>.Conflict(StaffMessages.DuplicateName);

        employee.Update(command.Input.FullName, command.Input.Position, command.UserId, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(employee.ToResponse());
    }
}
