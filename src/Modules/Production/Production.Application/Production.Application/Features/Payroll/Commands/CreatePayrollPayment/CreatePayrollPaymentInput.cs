namespace Production.Application.Features.Payroll.Commands.CreatePayrollPayment;

/// <summary><paramref name="RequestId"/> is made when the form opens: a repeated «Зберегти» adds nothing.</summary>
public sealed record CreatePayrollPaymentInput(int EmployeeId, DateOnly PaidOn, string Kind, decimal Amount,
    string? Comment, Guid? RequestId = null);
