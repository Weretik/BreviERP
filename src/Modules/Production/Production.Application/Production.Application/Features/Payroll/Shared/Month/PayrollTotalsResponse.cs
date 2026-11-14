namespace Production.Application.Features.Payroll.Shared.Month;

public sealed record PayrollTotalsResponse(int Minutes, decimal Sewing, decimal Cutting, decimal Bonus, decimal Master,
    decimal Chief, decimal Embroidery, decimal Other, decimal Accrued, decimal Paid, decimal Due, decimal Issued);
