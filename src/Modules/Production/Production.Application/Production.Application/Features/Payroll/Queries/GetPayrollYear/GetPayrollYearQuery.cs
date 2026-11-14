using Production.Application.Features.Orders.Shared.Access;

namespace Production.Application.Features.Payroll.Queries.GetPayrollYear;

/// <summary>«Історія за рік»: what each worker earned per month, up to the current month.</summary>
public sealed record GetPayrollYearQuery(ProductionActor Actor, int Year) : IQuery<Result<PayrollYearResponse>>;
