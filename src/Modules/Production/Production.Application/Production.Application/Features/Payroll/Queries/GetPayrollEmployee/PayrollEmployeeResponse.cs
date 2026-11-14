using Production.Application.Features.Payroll.Shared;
using Production.Application.Features.Payroll.Shared.Employee;
namespace Production.Application.Features.Payroll.Queries.GetPayrollEmployee;

/// <summary>A worker's month in detail: the work behind the sewing and cutting, payments and adjustments.</summary>
public sealed record PayrollEmployeeResponse(
    string Month,
    bool IsClosed,
    PayrollLineResponse Line,
    IReadOnlyList<PayrollWorkRowResponse> Work,
    IReadOnlyList<PayrollPaymentResponse> Payments,
    IReadOnlyList<PayrollAdjustmentResponse> Adjustments);
