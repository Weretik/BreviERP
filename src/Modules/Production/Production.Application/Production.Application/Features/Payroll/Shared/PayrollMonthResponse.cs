namespace Production.Application.Features.Payroll.Shared;

/// <summary>
/// «Зарплата» of a month «РРРР-ММ»: what every worker earned, was paid and is still due (PRD-D42). A closed month is
/// shown as it was when closed.
/// </summary>
public sealed record PayrollMonthResponse(
    string Month,
    bool IsClosed,
    DateTimeOffset? ClosedAt,
    string? ClosedByName,
    PayrollRatesResponse Rates,
    PayrollPersonResponse? Master,
    PayrollPersonResponse? Chief,
    int Stitches,
    PayrollPersonResponse? EmbroideryEmployee,
    decimal TotalSewing,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<PayrollLineResponse> Lines,
    PayrollTotalsResponse Totals,
    IReadOnlyList<PayrollPaymentResponse> Payments,
    IReadOnlyList<PayrollAdjustmentResponse> Adjustments,
    PayrollPermissionsResponse Permissions);
