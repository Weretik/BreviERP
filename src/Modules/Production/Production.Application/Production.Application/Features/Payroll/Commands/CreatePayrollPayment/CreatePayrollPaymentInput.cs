namespace Production.Application.Features.Payroll.Commands.CreatePayrollPayment;

public sealed record CreatePayrollPaymentInput(int EmployeeId, DateOnly PaidOn, string Kind, decimal Amount,
    string? Comment);
