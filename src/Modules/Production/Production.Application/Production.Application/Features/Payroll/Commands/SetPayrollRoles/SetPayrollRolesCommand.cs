using Production.Application.Abstractions;
using Production.Domain.Payroll;
using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Commands.SetPayrollRoles;

/// <summary>«Керівні частки»: who gets the master's and the chief's share; holds for every open month (PRD-D44).</summary>
public sealed record SetPayrollRolesCommand(ProductionActor Actor, string Month, SetPayrollRolesInput Input)
    : ICommand<Result<PayrollMonthResponse>>;
