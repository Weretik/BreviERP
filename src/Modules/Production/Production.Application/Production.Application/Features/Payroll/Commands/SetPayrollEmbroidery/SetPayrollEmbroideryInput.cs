namespace Production.Application.Features.Payroll.Commands.SetPayrollEmbroidery;

public sealed record SetPayrollEmbroideryInput(int Stitches, int? EmployeeId);
