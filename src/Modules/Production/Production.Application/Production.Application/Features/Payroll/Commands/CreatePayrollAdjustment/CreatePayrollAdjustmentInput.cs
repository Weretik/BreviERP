namespace Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;

public sealed record CreatePayrollAdjustmentInput(int EmployeeId, decimal Amount, string Reason);
