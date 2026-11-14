using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Commands.SetPayrollIssued;

/// <summary>«Видано»: the sum handed to a worker for the month; null clears it.</summary>
public sealed record SetPayrollIssuedCommand(ProductionActor Actor, string Month, SetPayrollIssuedInput Input)
    : ICommand<Result<PayrollMonthResponse>>;
