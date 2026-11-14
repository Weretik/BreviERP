namespace Production.Application.Features.Payroll.Shared;

/// <summary>A positive amount is an extra accrual, a negative one a deduction.</summary>
public sealed record PayrollAdjustmentResponse(int Id, int EmployeeId, string EmployeeName, decimal Amount,
    string Reason, DateTimeOffset CreatedAt, string CreatedByName, DateTimeOffset? VoidedAt, string? VoidedByName,
    string? VoidReason);
