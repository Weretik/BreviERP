using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.SetPayrollEmbroidery;

public sealed class SetPayrollEmbroideryHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<SetPayrollEmbroideryCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(SetPayrollEmbroideryCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: false, async change =>
        {
            if (command.Input.EmployeeId is { } id &&
                !await db.Employees.AnyAsync(x => x.Id == id && x.IsActive, cancellationToken))
                return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.ActiveEmployeeNotFound));
            return change.Period.SetEmbroidery(command.Input.Stitches, command.Input.EmployeeId, change.ActorName,
                change.Now).FailureOrNull();
        }, cancellationToken);
}
