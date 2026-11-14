using Production.Application.Abstractions;
using Production.Domain.Staff;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Staff.Shared;

namespace Production.Application.Features.Staff.Commands.CreateWorkshopEmployee;

public sealed class CreateWorkshopEmployeeHandler(IProductionDbContext db, TimeProvider clock)
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
