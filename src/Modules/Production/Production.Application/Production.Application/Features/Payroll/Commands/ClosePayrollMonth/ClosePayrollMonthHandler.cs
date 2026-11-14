using Production.Domain.Payroll;
using Production.Application.Abstractions;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Reports.Shared;
using Production.Application.Features.Payroll.Shared.Month;
using BuildingBlocks.Application;

namespace Production.Application.Features.Payroll.Commands.ClosePayrollMonth;

public sealed class ClosePayrollMonthHandler(PayrollChanges changes, PayrollSheets sheets,
    WorkshopReports reports)
    : ICommandHandler<ClosePayrollMonthCommand, Result<PayrollMonthResponse>>
{
    public async ValueTask<Result<PayrollMonthResponse>> Handle(ClosePayrollMonthCommand command,
        CancellationToken cancellationToken) =>
        await changes.ApplyAsync(command.Actor, command.Month, adminOnly: true, async change =>
        {
            var snapshot = await sheets.ReadAsync(change.Period.Month, cancellationToken);
            if (PayrollMessages.BlocksClosing(snapshot.Month.Warnings)) return Result.Conflict(PayrollMessages.HasWarnings);
            var closed = snapshot with
            {
                Month = snapshot.Month with
                {
                    IsClosed = true, ClosedAt = change.Now, ClosedByName = change.ActorName
                }
            };
            var report = await reports.CalculateAsync(change.Period.Month, cancellationToken);
            report = report with { ClosedMonths = [report.From], OpenMonths = [] };
            return change.Period.Close(PayrollSheets.Serialize(closed), WorkshopReports.Serialize(report),
                change.ActorName, change.Now).FailureOrNull();
        }, cancellationToken);
}
