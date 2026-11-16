using ActivityLog.Contracts.Activity;
using Production.Domain.Payroll;

namespace Production.Application.Features.Activity.EventHandlers;

/// <summary>How payroll payments and adjustments read in «Журнал дій» (ACT-001).</summary>
public sealed class PayrollActivity : IActivityDescriber
{
    public const string PaymentSubject = "payroll-payment";
    public const string AdjustmentSubject = "payroll-adjustment";

    public ActivityDescription? Describe(object domainEvent) => domainEvent switch
    {
        PayrollPaymentRecorded e => Of(PaymentSubject, "Виплата зарплати", e.Voided, e.Payment.Id, e.Payment.EmployeeName,
            e.Payment.Month, e.Voided ? e.Payment.VoidReason : Kind(e.Payment.Kind) + ": "
                + ActivityChanges.Number(e.Payment.Amount, "грн")),
        PayrollAdjustmentRecorded e => Of(AdjustmentSubject, "Коригування зарплати", e.Voided, e.Adjustment.Id,
            e.Adjustment.EmployeeName, e.Adjustment.Month, e.Voided ? e.Adjustment.VoidReason
                : ActivityChanges.Number(e.Adjustment.Amount, "грн") + ", " + e.Adjustment.Reason),
        _ => null,
    };

    private static string Kind(PayrollPaymentKind kind) => kind switch
    {
        PayrollPaymentKind.Advance => "Аванс",
        PayrollPaymentKind.Card => "На картку",
        PayrollPaymentKind.Cash => "Готівкою",
        _ => "Інше",
    };

    private static ActivityDescription Of(string subject, string title, bool voided, int id, string employee,
        DateOnly month, string? summary) =>
        new(voided ? ActivityActions.Voided : ActivityActions.Created, subject, title,
            id.ToString(CultureInfo.InvariantCulture),
            $"{employee} · {month.ToString("MM.yyyy", CultureInfo.InvariantCulture)}",
            voided ? "Сторновано: " + summary : summary, [], "Production");
}
