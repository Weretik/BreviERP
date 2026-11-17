using Production.Domain.Staff;
using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;
using BuildingBlocks.Application;

namespace Production.Application.Features.Payroll.Commands.SetPayrollEmbroidery;

public sealed class SetPayrollEmbroideryHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<SetPayrollEmbroideryCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(SetPayrollEmbroideryCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: false, async change =>
        {
            if (command.Input.EmployeeId is { } id &&
                !await db.Employees.AnyAsync(x => x.Id == new WorkshopEmployeeId(id) && x.IsActive, cancellationToken))
                return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.ActiveEmployeeNotFound));
            return change.Period.SetEmbroidery(command.Input.Stitches,
                command.Input.EmployeeId is { } employeeId ? new WorkshopEmployeeId(employeeId) : null, change.ActorName,
                change.Now).FailureOrNull();
        }, cancellationToken);
}
