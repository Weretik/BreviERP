using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared.Month;
using BuildingBlocks.Application;

namespace Production.Application.Features.Payroll.Shared;

/// <summary>What a change of a month needs: the tracked month, the person's name and the time.</summary>
public sealed record PayrollChange(PayrollMonth Period, string ActorName, DateTimeOffset Now);

/// <summary>
/// The common path of every change of «Зарплата»: access, a month that has started and is not closed, the change, the
/// fresh month back.
/// </summary>
public sealed class PayrollChanges(IProductionDbContext db, IProductionStaffReader staff, PayrollSheets sheets,
    TimeProvider clock)
{
    public async Task<Result<PayrollMonthResponse>> ApplyAsync(ProductionActor actor, string monthText, bool adminOnly,
        Func<PayrollChange, Task<Result?>> change, CancellationToken cancellationToken)
    {
        if (!(adminOnly ? actor.CanPay : actor.SeesPayroll))
            return Result<PayrollMonthResponse>.Forbidden(adminOnly ? PayrollMessages.CannotPay : PayrollMessages.CannotSee);
        PayrollMonths.TryParse(monthText, out var month);
        if (month > PayrollMonths.Current(clock)) return Result<PayrollMonthResponse>.Conflict(PayrollMessages.FutureMonth);
        var period = await PayrollMonths.TrackAsync(db, month, cancellationToken);
        if (period.IsClosed) return PayrollErrors.MonthClosed().ToResult<PayrollMonthResponse>();
        var actorName = await OrderActors.NameAsync(staff, actor.UserId, cancellationToken);
        if (actorName is null) return Result<PayrollMonthResponse>.Forbidden(PayrollMessages.CannotSee);
        var now = clock.GetUtcNow();
        var refused = await change(new PayrollChange(period, actorName, now));
        if (refused is not null) return Map(refused);
        if (!period.IsClosed && period.RecordChange(actorName, now).IsFailure)
            return PayrollErrors.MonthClosed().ToResult<PayrollMonthResponse>();
        var saved = await PayrollMonths.SaveAsync(db, cancellationToken);
        if (!saved.IsSuccess) return Map(saved);
        return Result.Success(await ReadAsync(actor, month, cancellationToken));
    }

    /// <summary>
    /// <see cref="ApplyAsync"/> for an admin's create with the form's request id: a repeat whose row
    /// <paramref name="exists"/> (also when the first one wins a race) returns the month as it is and adds nothing.
    /// </summary>
    public async Task<Result<PayrollMonthResponse>> ApplyOnceAsync(ProductionActor actor, string monthText,
        Guid? requestId, Func<Guid, CancellationToken, Task<bool>> exists, Func<PayrollChange, Task<Result?>> change,
        CancellationToken cancellationToken)
    {
        if (requestId is not { } id) return await ApplyAsync(actor, monthText, true, change, cancellationToken);
        if (!actor.CanPay) return Result<PayrollMonthResponse>.Forbidden(PayrollMessages.CannotPay);
        if (await exists(id, cancellationToken)) return await RepeatAsync(actor, monthText, cancellationToken);
        try
        {
            var result = await ApplyAsync(actor, monthText, true, change, cancellationToken);
            if (result.Status != ResultStatus.Conflict || !await exists(id, cancellationToken)) return result;
        }
        catch (DbUpdateException)
        {
            if (!await exists(id, cancellationToken)) throw;
        }

        return await RepeatAsync(actor, monthText, cancellationToken);
    }

    private async Task<Result<PayrollMonthResponse>> RepeatAsync(ProductionActor actor, string monthText,
        CancellationToken cancellationToken)
    {
        PayrollMonths.TryParse(monthText, out var month);
        return Result.Success(await ReadAsync(actor, month, cancellationToken));
    }

    public async Task<PayrollMonthResponse> ReadAsync(ProductionActor actor, DateOnly month,
        CancellationToken cancellationToken) =>
        PayrollSheets.Present((await sheets.ReadAsync(month, cancellationToken)).Month, actor, clock);

    private static Result<PayrollMonthResponse> Map(Result refused) => refused.Status switch
    {
        ResultStatus.Conflict => Result<PayrollMonthResponse>.Conflict([.. refused.Errors]),
        ResultStatus.NotFound => Result<PayrollMonthResponse>.NotFound([.. refused.Errors]),
        ResultStatus.Invalid => Result<PayrollMonthResponse>.Invalid([.. refused.ValidationErrors]),
        ResultStatus.Forbidden => Result<PayrollMonthResponse>.Forbidden([.. refused.Errors]),
        _ => Result<PayrollMonthResponse>.Error(string.Join(" ", refused.Errors))
    };
}
