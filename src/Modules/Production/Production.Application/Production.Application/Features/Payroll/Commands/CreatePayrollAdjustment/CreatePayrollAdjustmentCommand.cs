using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;

/// <summary>«Інше нарахування / утримання» of a month with a reason (PRD-D45).</summary>
public sealed record CreatePayrollAdjustmentCommand(ProductionActor Actor, string Month,
    CreatePayrollAdjustmentInput Input) : ICommand<Result<PayrollMonthResponse>>;
