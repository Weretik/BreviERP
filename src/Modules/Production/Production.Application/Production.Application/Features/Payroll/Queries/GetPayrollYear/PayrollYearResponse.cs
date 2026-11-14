namespace Production.Application.Features.Payroll.Queries.GetPayrollYear;

/// <summary>«Історія за рік»: what each worker earned per month.</summary>
public sealed record PayrollYearResponse(int Year, IReadOnlyList<PayrollYearMonthResponse> Months,
    IReadOnlyList<PayrollYearRowResponse> Rows);
