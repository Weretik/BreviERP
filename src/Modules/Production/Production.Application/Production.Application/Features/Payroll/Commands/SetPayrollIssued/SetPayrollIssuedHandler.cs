using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.SetPayrollIssued;

public sealed class SetPayrollIssuedHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<SetPayrollIssuedCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(SetPayrollIssuedCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: true, async change =>
        {
            var input = command.Input;
            if (!await db.Employees.AnyAsync(x => x.Id == input.EmployeeId, cancellationToken))
                return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.EmployeeNotFound));
            var month = change.Period.Month;
            var row = await db.PayrollIssued.SingleOrDefaultAsync(x => x.Month == month && x.EmployeeId == input.EmployeeId,
                cancellationToken);
            if (input.Amount is not { } amount)
            {
                if (row is not null) db.PayrollIssued.Remove(row);
                return null;
            }

            if (row is null)
            {
                row = new PayrollIssued(month, input.EmployeeId);
                db.PayrollIssued.Add(row);
            }

            return row.Set(amount, change.ActorName, change.Now).FailureOrNull();
        }, cancellationToken);
}
