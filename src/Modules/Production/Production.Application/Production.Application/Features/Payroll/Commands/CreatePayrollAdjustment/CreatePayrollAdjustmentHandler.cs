using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;
using BuildingBlocks.Application;

namespace Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;

public sealed class CreatePayrollAdjustmentHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<CreatePayrollAdjustmentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(CreatePayrollAdjustmentCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyOnceAsync(command.Actor, command.Month, command.Input.RequestId,
            (id, ct) => db.PayrollAdjustments.AsNoTracking().AnyAsync(x => x.RequestId == id, ct), async change =>
        {
            var input = command.Input;
            var employee = await db.Employees.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == input.EmployeeId, cancellationToken);
            if (employee is null) return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.EmployeeNotFound));
            var adjustment = PayrollAdjustment.Create(
                new PayrollEntryTarget(change.Period.Month, employee.Id, employee.FullName), input.Amount, input.Reason,
                change.ActorName, change.Now, input.RequestId);
            if (adjustment.IsFailure) return adjustment.Error!.ToResult();
            db.PayrollAdjustments.Add(adjustment.Value);
            return null;
        }, cancellationToken);
}
