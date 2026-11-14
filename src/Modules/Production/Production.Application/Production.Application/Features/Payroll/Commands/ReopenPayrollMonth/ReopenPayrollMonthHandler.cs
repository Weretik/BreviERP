using Production.Domain.Payroll;
using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;
using BuildingBlocks.Application;

namespace Production.Application.Features.Payroll.Commands.ReopenPayrollMonth;

public sealed class ReopenPayrollMonthHandler(IProductionDbContext db, IProductionStaffReader staff,
    PayrollChanges changes, TimeProvider clock) : ICommandHandler<ReopenPayrollMonthCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(ReopenPayrollMonthCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanPay) return Result<PayrollMonthResponse>.Forbidden(PayrollMessages.CannotPay);
        PayrollMonths.TryParse(command.Month, out var month);
        var period = await db.PayrollMonths.SingleOrDefaultAsync(x => x.Month == month, cancellationToken);
        if (period is null) return PayrollErrors.MonthNotClosed().ToResult<PayrollMonthResponse>();
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<PayrollMonthResponse>.Forbidden(PayrollMessages.CannotPay);
        var reopened = period.Reopen(actorName, clock.GetUtcNow());
        if (reopened.IsFailure) return reopened.Error!.ToResult<PayrollMonthResponse>();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await changes.ReadAsync(command.Actor, month, cancellationToken));
    }
}
