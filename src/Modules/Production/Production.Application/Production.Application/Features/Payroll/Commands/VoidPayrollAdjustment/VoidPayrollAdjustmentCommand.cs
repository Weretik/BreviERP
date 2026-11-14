using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.VoidPayrollAdjustment;

/// <summary>Cancels an adjustment with a reason.</summary>
public sealed record VoidPayrollAdjustmentCommand(ProductionActor Actor, int AdjustmentId, VoidPayrollEntryInput Input)
    : ICommand<Result<PayrollMonthResponse>>;
