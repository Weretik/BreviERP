using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Domain.Staff;

namespace Production.Application.Staff.CreateEmployee;

public sealed class CreateWorkshopEmployeeCommandHandler(IProductionDbContext db, TimeProvider clock)
    : ICommandHandler<CreateWorkshopEmployeeCommand, Result<WorkshopEmployeeResponse>>
{
    public async ValueTask<Result<WorkshopEmployeeResponse>> Handle(CreateWorkshopEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        if (await EmployeeLookup.ActiveNameTakenAsync(db, command.Input.FullName, null, cancellationToken))
            return Result<WorkshopEmployeeResponse>.Conflict(StaffMessages.DuplicateName);

        var created = WorkshopEmployee.Create(command.Input.FullName, command.Input.Position, command.UserId,
            clock.GetUtcNow());
        if (created.IsFailure) return created.Error!.ToResult<WorkshopEmployeeResponse>();
        var employee = created.Value;
        db.Employees.Add(employee);
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(employee.ToResponse());
    }
}
