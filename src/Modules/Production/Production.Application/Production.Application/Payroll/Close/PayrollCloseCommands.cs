using Production.Application.Abstractions;
using Production.Application.Contracts;
using Production.Application.Orders;
using Production.Application.Orders.Access;
using Production.Application.Orders.Validation;

namespace Production.Application.Payroll.Close;

/// <summary>
/// «Закрити розрахунковий місяць» (PRD-D43): keeps the calculation as it is now and locks the month. Refused while the
/// calculation has warnings.
/// </summary>
public sealed record ClosePayrollMonthCommand(ProductionActor Actor, string Month) : ICommand<Result<PayrollMonthResponse>>;

public sealed class ClosePayrollMonthCommandValidator : AbstractValidator<ClosePayrollMonthCommand>
{
    public ClosePayrollMonthCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
    }
}

public sealed class ClosePayrollMonthCommandHandler(PayrollChanges changes, PayrollSheets sheets)
    : ICommandHandler<ClosePayrollMonthCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(ClosePayrollMonthCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: true, async change =>
        {
            var snapshot = await sheets.ReadAsync(change.Period.Month, cancellationToken);
            if (snapshot.Month.Warnings.Count > 0) return Result.Conflict(PayrollMessages.HasWarnings);
            var closed = snapshot with
            {
                Month = snapshot.Month with
                {
                    IsClosed = true, ClosedAt = change.Now, ClosedByName = change.ActorName
                }
            };
            change.Period.Close(PayrollSheets.Serialize(closed), change.ActorName, change.Now);
            return null;
        }, cancellationToken);
}

/// <summary>Opens a closed month again (Admin): it is counted afresh from the current data.</summary>
public sealed record ReopenPayrollMonthCommand(ProductionActor Actor, string Month)
    : ICommand<Result<PayrollMonthResponse>>;

public sealed class ReopenPayrollMonthCommandValidator : AbstractValidator<ReopenPayrollMonthCommand>
{
    public ReopenPayrollMonthCommandValidator()
    {
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Month).Must(x => PayrollMonths.TryParse(x, out _)).WithMessage(PayrollMessages.Month);
    }
}

public sealed class ReopenPayrollMonthCommandHandler(IProductionDbContext db, IProductionStaffReader staff,
    PayrollChanges changes, TimeProvider clock) : ICommandHandler<ReopenPayrollMonthCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(ReopenPayrollMonthCommand command,
        CancellationToken cancellationToken)
    {
        if (!command.Actor.CanPay) return Result<PayrollMonthResponse>.Forbidden(PayrollMessages.CannotPay);
        PayrollMonths.TryParse(command.Month, out var month);
        var period = await db.PayrollMonths.SingleOrDefaultAsync(x => x.Month == month, cancellationToken);
        if (period is not { IsClosed: true }) return Result<PayrollMonthResponse>.Conflict(PayrollMessages.NotClosed);
        var actorName = await OrderActors.NameAsync(staff, command.Actor.UserId, cancellationToken);
        if (actorName is null) return Result<PayrollMonthResponse>.Forbidden(PayrollMessages.CannotPay);
        period.Reopen(actorName, clock.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success(await changes.ReadAsync(command.Actor, month, cancellationToken));
    }
}
