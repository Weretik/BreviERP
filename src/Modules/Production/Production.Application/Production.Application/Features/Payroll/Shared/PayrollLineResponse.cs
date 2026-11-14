namespace Production.Application.Features.Payroll.Shared;

public sealed record PayrollLineResponse(
    int EmployeeId,
    string Name,
    string Position,
    bool IsActive,
    int Minutes,
    int Sewn,
    decimal Sewing,
    int Cut,
    decimal Cutting,
    decimal Bonus,
    decimal Master,
    decimal Chief,
    decimal Embroidery,
    decimal Other,
    decimal Accrued,
    decimal Paid,
    decimal Due,
    decimal? Issued);
