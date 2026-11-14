namespace Production.Application.Features.Payroll.Queries.GetPayrollYear;

/// <summary><c>Accrued</c> has 12 values, January first; null where the worker earned nothing.</summary>
public sealed record PayrollYearRowResponse(int EmployeeId, string Name, string Position,
    IReadOnlyList<decimal?> Accrued, decimal Total);
