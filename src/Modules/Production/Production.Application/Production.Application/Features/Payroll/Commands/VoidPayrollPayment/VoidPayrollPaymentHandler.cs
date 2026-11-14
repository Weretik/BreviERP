using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Commands.VoidPayrollPayment;

public sealed class VoidPayrollPaymentHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<VoidPayrollPaymentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(VoidPayrollPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var payment = await db.PayrollPayments.SingleOrDefaultAsync(x => x.Id == command.PaymentId, cancellationToken);
        if (payment is null) return Result<PayrollMonthResponse>.NotFound(PayrollMessages.PaymentNotFound);
        return await changes.ApplyAsync(command.Actor, PayrollMonths.Format(payment.Month), adminOnly: true, change =>
            Task.FromResult(payment.Void(command.Input.Reason, change.ActorName, change.Now).FailureOrNull()),
            cancellationToken);
    }
}
