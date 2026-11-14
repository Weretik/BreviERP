namespace Production.Application.Features.Payroll.Shared;

public sealed record PayrollPermissionsResponse(bool CanSetUp, bool CanPay, bool CanClose, bool CanReopen);
