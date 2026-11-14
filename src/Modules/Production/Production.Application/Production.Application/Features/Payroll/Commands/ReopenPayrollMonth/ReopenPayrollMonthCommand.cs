using Production.Domain.Payroll;
using Production.Application.Abstractions;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;

namespace Production.Application.Features.Payroll.Commands.ReopenPayrollMonth;

/// <summary>Opens a closed month again (Admin): it is counted afresh from the current data.</summary>
public sealed record ReopenPayrollMonthCommand(ProductionActor Actor, string Month)
    : ICommand<Result<PayrollMonthResponse>>;
