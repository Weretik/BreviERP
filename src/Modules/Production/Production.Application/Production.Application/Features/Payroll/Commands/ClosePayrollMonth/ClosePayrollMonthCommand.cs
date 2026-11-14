using Production.Domain.Payroll;
using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Commands.ClosePayrollMonth;

/// <summary>
/// «Закрити розрахунковий місяць» (PRD-D43): keeps the calculation and the report of the month (PRD-D49) as they are now
/// and locks the month. Refused while the calculation has a warning other than an unchosen master or chief, whose share
/// is then simply not paid.
/// </summary>
public sealed record ClosePayrollMonthCommand(ProductionActor Actor, string Month) : ICommand<Result<PayrollMonthResponse>>;
