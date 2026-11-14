namespace Production.Application.Features.Payroll.Shared.Month;

public sealed record PayrollPermissionsResponse(bool CanSetUp, bool CanPay, bool CanClose, bool CanReopen);
