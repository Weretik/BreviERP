using Production.Application.Abstractions;
using Production.Application.Contracts;

namespace Production.Application.PayrollParameters.GetPayrollParameters;

/// <summary>«Правила зарплати»: the payroll coefficients as they stand in «Параметри ціноутворення» (read only).</summary>
public sealed record GetPayrollParametersQuery : IQuery<Result<IReadOnlyList<PayrollParameterResponse>>>;
