using BuildingBlocks.Domain.Errors;

namespace Production.Domain.Payroll;

/// <summary>Business rule errors of «Зарплата».</summary>
public static class PayrollErrors
{
    public static Error MonthClosed() =>
        Error.Conflict("production.payroll_month_closed",
            "Місяць зарплати закрито: змінювати роботу, години й виплати в ньому не можна.");

    public static Error MonthNotClosed() =>
        Error.Conflict("production.payroll_month_open", "Місяць ще не закрито.");

    public static Error AlreadyVoided() =>
        Error.Conflict("production.payroll_already_voided", "Цей запис уже скасовано.");

    public static Error AmountOutOfRange() =>
        Error.Validation("production.payroll_amount", "Сума від 0,01 до 10 000 000 грн, з точністю до копійки.", "amount");

    public static Error AdjustmentAmountOutOfRange() =>
        Error.Validation("production.payroll_adjustment_amount",
            "Сума не нуль, до 10 000 000 грн, з точністю до копійки; утримання — з мінусом.", "amount");

    public static Error IssuedAmountOutOfRange() =>
        Error.Validation("production.payroll_issued_amount", "Сума від 0 до 10 000 000 грн, з точністю до копійки.", "amount");

    public static Error StitchesOutOfRange() =>
        Error.Validation("production.payroll_stitches", "Кількість стібків — ціле число від 0.", "stitches");
}
