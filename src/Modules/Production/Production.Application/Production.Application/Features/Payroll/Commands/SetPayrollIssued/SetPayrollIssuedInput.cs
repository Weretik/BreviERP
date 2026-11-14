namespace Production.Application.Features.Payroll.Commands.SetPayrollIssued;

/// <summary>Null clears «Видано».</summary>
public sealed record SetPayrollIssuedInput(int EmployeeId, decimal? Amount);
