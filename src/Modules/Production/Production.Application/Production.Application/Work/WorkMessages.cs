namespace Production.Application.Work;

internal static class WorkMessages
{
    public const string CannotSee = "Журнал робіт бачать начальник цеху й адміністратор.";
    public const string CannotRecord = "Фіксувати роботу може начальник цеху або адміністратор у завданні «Нове».";
    public const string CannotImport = "Переносити журнал старого «Цеху» може лише адміністратор.";
    public const string NotNew = "Завдання вже не «Нове»: роботу не фіксуємо.";
    public const string NoRate = "У позиції немає розцінки пошиття. Вкажіть її в картці позиції перед обліком роботи.";
    public const string EmployeeNotFound = "Працівника немає серед тих, хто працює.";
    public const string WorkNotFound = "Запису роботи не знайдено.";
    public const string FutureDate = "Дата роботи не може бути пізніше сьогодні.";
    public const string Month = "Місяць у форматі РРРР-ММ.";
    public const string Reason = "Для роботи понад план оберіть причину.";

    public static string OverPlan(string kind, int remaining) =>
        $"За позицією залишилось {kind} {remaining} шт. Більше оформіть як роботу понад план з причиною.";
}

/// <summary>The calendar day in Kyiv: work dates and the forecast start are workshop days.</summary>
public static class WorkshopDay
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DateOnly Today(TimeProvider clock) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Kyiv).DateTime);
}
