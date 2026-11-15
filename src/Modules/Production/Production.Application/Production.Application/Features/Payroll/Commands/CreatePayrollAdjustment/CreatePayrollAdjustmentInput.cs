namespace Production.Application.Features.Payroll.Commands.CreatePayrollAdjustment;

/// <summary><paramref name="RequestId"/> is made when the form opens: a repeated «Зберегти» adds nothing.</summary>
public sealed record CreatePayrollAdjustmentInput(int EmployeeId, decimal Amount, string Reason, Guid? RequestId = null);
