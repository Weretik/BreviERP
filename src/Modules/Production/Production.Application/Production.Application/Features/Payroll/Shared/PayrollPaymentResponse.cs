namespace Production.Application.Features.Payroll.Shared;

public sealed record PayrollPaymentResponse(int Id, int EmployeeId, string EmployeeName, DateOnly PaidOn, string Kind,
    decimal Amount, string? Comment, DateTimeOffset CreatedAt, string CreatedByName, DateTimeOffset? VoidedAt,
    string? VoidedByName, string? VoidReason);
