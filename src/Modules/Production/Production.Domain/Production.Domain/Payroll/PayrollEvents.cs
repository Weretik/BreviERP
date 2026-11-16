using BuildingBlocks.Domain.Events;

namespace Production.Domain.Payroll;

/// <summary>A payroll payment was recorded or voided (ACT-001).</summary>
public sealed record PayrollPaymentRecorded(PayrollPayment Payment, bool Voided) : IDomainEvent;

/// <summary>A payroll adjustment was recorded or voided (ACT-001).</summary>
public sealed record PayrollAdjustmentRecorded(PayrollAdjustment Adjustment, bool Voided) : IDomainEvent;
