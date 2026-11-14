namespace Production.Application.Features.Payroll.Commands.SetPayrollRoles;

public sealed record SetPayrollRolesInput(int? MasterEmployeeId, int? ChiefEmployeeId);
