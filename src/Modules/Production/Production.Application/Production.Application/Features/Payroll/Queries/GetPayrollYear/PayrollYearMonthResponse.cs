namespace Production.Application.Features.Payroll.Queries.GetPayrollYear;

public sealed record PayrollYearMonthResponse(string Month, bool IsClosed, decimal Accrued);
