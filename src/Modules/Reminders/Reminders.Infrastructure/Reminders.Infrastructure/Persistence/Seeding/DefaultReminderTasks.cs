using Reminders.Domain.Schedules;
using Reminders.Domain.Tasks;

namespace Reminders.Infrastructure.Persistence.Seeding;

/// <summary>The agreed list of 25 tasks (item 12 of the brief is split into payment and report). Editable in Admin.</summary>
internal static class DefaultReminderTasks
{
    private const ReminderOrganization Brevi = ReminderOrganization.PpBrevi;
    private const ReminderOrganization Fop = ReminderOrganization.FopSohokon;
    private const ReminderOrganization Personal = ReminderOrganization.None;
    private const ReminderPeriodicity Month = ReminderPeriodicity.Monthly;
    private const ReminderPeriodicity Quarter = ReminderPeriodicity.Quarterly;
    private const ReminderPeriodicity Year = ReminderPeriodicity.Yearly;
    private const int Lead = 5;

    public static IReadOnlyList<(string Title, ReminderOrganization Organization, ReminderSchedule Schedule)> All { get; } =
    [
        ("Реєстрація податкових накладних за 1–15 число", Brevi, S(Month, DueRule.DayOfMonth(5, 1))),
        ("Реєстрація податкових накладних за 16–останнє число", Brevi, S(Month, DueRule.DayOfMonth(18, 1))),
        ("Декларація з ПДВ", Brevi, S(Month, DueRule.DayOfMonth(20, 1))),
        ("Звіт з ЄСВ", Brevi, S(Month, DueRule.DayOfMonth(20, 1))),
        ("Сплата ПДВ", Brevi, S(Month, DueRule.DayOfMonth(30, 1))),
        ("Звіт по особах з інвалідністю", Brevi, S(Quarter, DueRule.DaysAfterEnd(40))),
        ("Річний баланс", Brevi, S(Year, DueRule.DayOfMonth(28, 2))),
        ("Сплата податку на прибуток", Brevi, S(Year, DueRule.DaysAfterEnd(40))),
        ("Перерахування зарплати (аванс)", Brevi, S(Month, DueRule.DayOfMonth(15, 0))),
        ("Перерахування залишку зарплати", Brevi, S(Month, DueRule.LastWorkingDay(0))),
        ("ЄСВ за себе", Fop, S(Quarter, DueRule.DayOfMonth(20, 1))),
        ("Сплата ЄСВ за працівників", Fop, S(Month, DueRule.DayOfMonth(20, 1))),
        ("Звіт з ЄСВ за працівників", Fop, S(Quarter, DueRule.DaysAfterEnd(40))),
        ("Декларація єдиного податку", Fop, S(Quarter, DueRule.DaysAfterEnd(40))),
        ("Сплата єдиного податку", Fop, S(Quarter, DueRule.DaysAfterEnd(50))),
        ("Сплата військового збору", Fop, S(Quarter, DueRule.DaysAfterEnd(50))),
        ("Звіт по особах з інвалідністю", Fop, S(Quarter, DueRule.DaysAfterEnd(40))),
        ("Перерахування зарплати (аванс)", Fop, S(Month, DueRule.DayOfMonth(15, 0))),
        ("Перерахування залишку зарплати", Fop, S(Month, DueRule.LastWorkingDay(0))),
        ("Порахувати зарплату офісу за попередній місяць", Personal, S(Month, DueRule.DayOfMonth(18, 1))),
        ("Видати зарплату офісу", Personal, S(Month, DueRule.DayOfMonth(20, 1))),
        ("Перерахувати оренду цеху", Personal, S(Month, DueRule.DayOfMonth(12, 0))),
        ("Сплатити оренду цеху", Personal, S(Month, DueRule.DayOfMonth(15, 0))),
        ("Порахувати зарплату цеху за попередній місяць", Personal, S(Month, DueRule.DayOfMonth(1, 1))),
        ("Підготувати готівку і видати залишок зарплати цеху за попередній місяць", Personal, S(Month, DueRule.DayOfMonth(5, 1)))
    ];

    private static ReminderSchedule S(ReminderPeriodicity periodicity, DueRule due) => new(periodicity, due, Lead);
}
