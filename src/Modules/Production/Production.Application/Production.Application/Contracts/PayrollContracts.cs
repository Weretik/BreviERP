namespace Production.Application.Contracts;

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

/// <summary>Shares from «Параметри ціноутворення» as fractions (0.08 = 8 %); null when not filled in.</summary>
public sealed record PayrollRatesResponse(decimal? Bonus, decimal? BonusThreshold, decimal? Cutting, decimal? Master,
    decimal? Chief, decimal? Embroidery);

/// <summary><c>IsAssigned</c> is false when the person is found by position, not chosen.</summary>
public sealed record PayrollPersonResponse(int EmployeeId, string Name, bool IsAssigned);

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

public sealed record PayrollTotalsResponse(int Minutes, decimal Sewing, decimal Cutting, decimal Bonus, decimal Master,
    decimal Chief, decimal Embroidery, decimal Other, decimal Accrued, decimal Paid, decimal Due, decimal Issued);

public sealed record PayrollPaymentResponse(int Id, int EmployeeId, string EmployeeName, DateOnly PaidOn, string Kind,
    decimal Amount, string? Comment, DateTimeOffset CreatedAt, string CreatedByName, DateTimeOffset? VoidedAt,
    string? VoidedByName, string? VoidReason);

/// <summary>A positive amount is an extra accrual, a negative one a deduction.</summary>
public sealed record PayrollAdjustmentResponse(int Id, int EmployeeId, string EmployeeName, decimal Amount,
    string Reason, DateTimeOffset CreatedAt, string CreatedByName, DateTimeOffset? VoidedAt, string? VoidedByName,
    string? VoidReason);

public sealed record PayrollPermissionsResponse(bool CanSetUp, bool CanPay, bool CanClose, bool CanReopen);

/// <summary>A worker's month in detail: the work behind the sewing and cutting, payments and adjustments.</summary>
public sealed record PayrollEmployeeResponse(
    string Month,
    bool IsClosed,
    PayrollLineResponse Line,
    IReadOnlyList<PayrollWorkRowResponse> Work,
    IReadOnlyList<PayrollPaymentResponse> Payments,
    IReadOnlyList<PayrollAdjustmentResponse> Adjustments);

/// <summary>Not cancelled work; <c>Rate</c> is the one the month is paid by.</summary>
public sealed record PayrollWorkRowResponse(int WorkId, int EmployeeId, DateOnly WorkDate, int OrderId,
    int OrderNumber, string ModelName, string Kind, int Quantity, decimal Rate, decimal Amount, bool OverPlan);

/// <summary>«Історія за рік»: what each worker earned per month.</summary>
public sealed record PayrollYearResponse(int Year, IReadOnlyList<PayrollYearMonthResponse> Months,
    IReadOnlyList<PayrollYearRowResponse> Rows);

public sealed record PayrollYearMonthResponse(string Month, bool IsClosed, decimal Accrued);

/// <summary><c>Accrued</c> has 12 values, January first; null where the worker earned nothing.</summary>
public sealed record PayrollYearRowResponse(int EmployeeId, string Name, string Position,
    IReadOnlyList<decimal?> Accrued, decimal Total);

public sealed record SetPayrollRolesInput(int? MasterEmployeeId, int? ChiefEmployeeId);

public sealed record SetPayrollEmbroideryInput(int Stitches, int? EmployeeId);

public sealed record CreatePayrollPaymentInput(int EmployeeId, DateOnly PaidOn, string Kind, decimal Amount,
    string? Comment);

public sealed record CreatePayrollAdjustmentInput(int EmployeeId, decimal Amount, string Reason);

public sealed record VoidPayrollEntryInput(string Reason);

/// <summary>Null clears «Видано».</summary>
public sealed record SetPayrollIssuedInput(int EmployeeId, decimal? Amount);
