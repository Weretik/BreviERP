using Production.Domain.Payroll;

namespace Production.Application.Payroll;

internal static class PayrollMessages
{
    public const string CannotSee = "Зарплату бачать начальник цеху й адміністратор.";
    public const string CannotPay = "Виплати, утримання й закриття місяця веде адміністратор.";
    public const string Month = "Місяць у форматі РРРР-ММ.";
    public const string FutureMonth = "Цей місяць ще не настав.";
    public const string Closed = "Місяць зарплати закрито: змінювати роботу, години й виплати в ньому не можна.";
    public const string NotClosed = "Місяць ще не закрито.";
    public const string HasWarnings = "Місяць не закрито: спершу виправте попередження розрахунку.";
    public const string EmployeeNotFound = "Працівника не знайдено.";
    public const string ActiveEmployeeNotFound = "Працівника немає серед тих, хто працює.";
    public const string PaymentNotFound = "Виплату не знайдено.";
    public const string AdjustmentNotFound = "Нарахування чи утримання не знайдено.";
    public const string AlreadyVoided = "Цей запис уже скасовано.";
    public const string FuturePayment = "Дата виплати не може бути пізніше сьогодні.";
    public const string Amount = "Сума від 0,01 до 10 000 000 грн, з точністю до копійки.";
    public const string AdjustmentAmount = "Сума не нуль, до 10 000 000 грн, з точністю до копійки; утримання — з мінусом.";
    public const string Reason = "Вкажіть причину (від 3 символів).";
    public const string PaymentKind = "Оберіть вид виплати.";
    public const string Stitches = "Кількість стібків — ціле число від 0.";
    public const string Year = "Рік від 2020 до 2100.";

    public static string Warning(PayrollWarning warning) => warning switch
    {
        PayrollWarning.NoMaster => "Не обрано майстра цеху: його частка не нарахована.",
        PayrollWarning.NoChief => "Не обрано начальника цеху: його частка не нарахована.",
        PayrollWarning.NoEmbroideryWorker => "Є стібки вишивки, але не обрано, кому їх нарахувати.",
        PayrollWarning.NoBonusRate => Missing("Премія швачки"),
        PayrollWarning.NoBonusThreshold => Missing("Поріг фактичної виплати премії швачки"),
        PayrollWarning.NoCuttingRate => Missing("Коефіцієнт крою"),
        PayrollWarning.NoMasterRate => Missing("Коефіцієнт майстра"),
        PayrollWarning.NoChiefRate => Missing("Коефіцієнт начальника цеху"),
        PayrollWarning.NoEmbroideryRate => Missing("Оплата фактичної вишивки за 1000 стібків"),
        _ => warning.ToString()
    };

    private static string Missing(string name) => $"У «Параметрах ціноутворення» не заповнено «{name}».";
}
