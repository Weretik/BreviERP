using Production.Application.Abstractions;
using Production.Domain.Timesheet;

namespace Production.Application.Features.Timesheet.Shared;

internal static class TimesheetMessages
{
    public const string CannotSee = "«Робочий час» веде начальник цеху або адміністратор.";
    public const string FutureDate = "Не можна відмічати години на день, що ще не настав.";
    public const string Minutes = "Години від 0 до 24 з кроком 0,25 (15 хвилин). Для відсутності вкажіть 0.";
    public const string Stale = "Години цього працівника вже змінив хтось інший. Оновіть сторінку й внесіть зміни ще раз.";
    public const string EmployeeNotFound = "Працівника не знайдено.";
    public const string Duplicate = "Працівник указаний двічі.";
}
