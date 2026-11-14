using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;
using BuildingBlocks.Application;

namespace Production.Application.Features.Payroll.Commands.VoidPayrollAdjustment;

public sealed class VoidPayrollAdjustmentHandler(IProductionDbContext db, PayrollChanges changes)
    : ICommandHandler<VoidPayrollAdjustmentCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(VoidPayrollAdjustmentCommand command,
        CancellationToken cancellationToken)
    {
        var adjustment = await db.PayrollAdjustments.SingleOrDefaultAsync(x => x.Id == command.AdjustmentId,
            cancellationToken);
        if (adjustment is null) return Result<PayrollMonthResponse>.NotFound(PayrollMessages.AdjustmentNotFound);
        return await changes.ApplyAsync(command.Actor, PayrollMonths.Format(adjustment.Month), adminOnly: true, change =>
            Task.FromResult(adjustment.Void(command.Input.Reason, change.ActorName, change.Now).FailureOrNull()),
            cancellationToken);
    }
}
