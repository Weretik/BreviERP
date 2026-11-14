using K = Scheduling.Application.Features.Catalog.Shared.ScheduledTaskKeys;

namespace Scheduling.Application.Features.Catalog.Shared;

/// <summary>
/// Every background task of Brevi, in the order of the «Фонові задачі» page (SCH-001 R-01). Adding a task: a key here,
/// a runner in Host.Seed (Jobs/Tasks), its secrets on the job in the deploy workflow. The admin cannot add, rename or
/// delete tasks. Tasks of the old brevierp-sync and brevierp-reminders jobs start disabled until the switch-over (R-12).
/// </summary>
public static class ScheduledTaskCatalog
{
    private static readonly TaskSchedule WeekdaysAtNine = TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0));

    public static IReadOnlyList<ScheduledTaskDefinition> Tasks { get; } =
    [
        new(K.Outbox, "Досилання повідомлень",
            "Повторює повідомлення, які не пішли з першого разу: заявки з сайту, «Надіслати в цех», параметри цін.",
            TaskSchedule.Interval(1).Value, MaxMinutes: 5, Editable: false, RunsInTick: true),
        new(K.Reminders, "Нагадування в Telegram", "Надсилає нагадування про строки в групи Telegram.",
            WeekdaysAtNine, 30, EnabledByDefault: false),
        new(K.KeepinSuppliers, "Постачальники з KeepinCRM", "Оновлює постачальників з KeepinCRM.",
            WeekdaysAtNine, 30, EnabledByDefault: false),
        new(K.KeepinClients, "Клієнти з KeepinCRM", "Оновлює клієнтів з KeepinCRM.",
            WeekdaysAtNine, 30, EnabledByDefault: false),
        new(K.SewingPrices, "Ціни пошиву", "Публікує ціни пошиву, якщо щось змінилося поза адмінкою.",
            WeekdaysAtNine, 30, EnabledByDefault: false),
        new(K.MediaThumbnails, "Прев'ю фото", "Робить прев'ю для фото бібліотеки, у яких його ще немає.",
            WeekdaysAtNine, 30, EnabledByDefault: false),
        new(K.PurchaseInvoices, "Рахунки на закупку", "Переносить рахунки на закупку з таблиці «финансы».",
            WeekdaysAtNine, 30, EnabledByDefault: false),
        new(K.Leads, "Ліди", "Переносить ліди з таблиці «Лиды Brevi».",
            WeekdaysAtNine, 30, EnabledByDefault: false),
        new(K.PpeSupplierPrices, "Ціни постачальників ЗІЗ",
            "Оновлює ціни постачальників за розкладом «Імпорт цін»: який день для якого постачальника, вирішує «Імпорт цін».",
            WeekdaysAtNine, 60, EnabledByDefault: false),
    ];

    private static readonly Dictionary<string, ScheduledTaskDefinition> ByKey =
        Tasks.ToDictionary(x => x.Key, StringComparer.Ordinal);

    public static ScheduledTaskDefinition? Find(string key) => ByKey.GetValueOrDefault(key);
}
