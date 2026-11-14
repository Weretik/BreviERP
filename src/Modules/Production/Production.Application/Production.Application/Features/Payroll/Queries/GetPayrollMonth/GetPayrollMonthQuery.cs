using Production.Application.Features.Orders.Shared.Access;
using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Month;

namespace Production.Application.Features.Payroll.Queries.GetPayrollMonth;

/// <summary>«Зарплата» of a month «РРРР-ММ».</summary>
public sealed record GetPayrollMonthQuery(ProductionActor Actor, string Month) : IQuery<Result<PayrollMonthResponse>>;
