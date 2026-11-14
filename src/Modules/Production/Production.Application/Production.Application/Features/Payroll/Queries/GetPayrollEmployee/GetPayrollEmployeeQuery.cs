using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Payroll.Queries.GetPayrollEmployee;

/// <summary>A worker's month in detail.</summary>
public sealed record GetPayrollEmployeeQuery(ProductionActor Actor, string Month, int EmployeeId)
    : IQuery<Result<PayrollEmployeeResponse>>;
