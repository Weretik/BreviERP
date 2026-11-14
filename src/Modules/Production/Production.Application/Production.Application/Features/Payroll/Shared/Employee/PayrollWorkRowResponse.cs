namespace Production.Application.Features.Payroll.Shared.Employee;

/// <summary>Not cancelled work; <c>Rate</c> is the one the month is paid by.</summary>
public sealed record PayrollWorkRowResponse(int WorkId, int EmployeeId, DateOnly WorkDate, int OrderId,
    int OrderNumber, string ModelName, string Kind, int Quantity, decimal Rate, decimal Amount, bool OverPlan);
