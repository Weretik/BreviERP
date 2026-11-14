namespace Production.Application.Features.Payroll.Shared;

/// <summary><c>IsAssigned</c> is false when the person is found by position, not chosen.</summary>
public sealed record PayrollPersonResponse(int EmployeeId, string Name, bool IsAssigned);
