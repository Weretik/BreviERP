using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Work.Shared;
using Production.Application.Features.Payroll.Shared.Month;
using BuildingBlocks.Application;

namespace Production.Application.Features.Payroll.Commands.CreatePayrollPayment;

public sealed class CreatePayrollPaymentHandler(IProductionDbContext db, PayrollChanges changes,
    TimeProvider clock) : ICommandHandler<CreatePayrollPaymentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(CreatePayrollPaymentCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: true, async change =>
        {
            var input = command.Input;
            if (input.PaidOn > WorkshopDay.Today(clock))
                return Result.Invalid(new ValidationError("Input.PaidOn", PayrollMessages.FuturePayment));
            var employee = await db.Employees.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == input.EmployeeId, cancellationToken);
            if (employee is null) return Result.Invalid(new ValidationError("Input.EmployeeId", PayrollMessages.EmployeeNotFound));
            var payment = PayrollPayment.Create(
                new PayrollEntryTarget(change.Period.Month, employee.Id, employee.FullName), input.PaidOn,
                Enum.Parse<PayrollPaymentKind>(input.Kind), input.Amount, input.Comment, change.ActorName, change.Now);
            if (payment.IsFailure) return payment.Error!.ToResult();
            db.PayrollPayments.Add(payment.Value);
            return null;
        }, cancellationToken);
}
