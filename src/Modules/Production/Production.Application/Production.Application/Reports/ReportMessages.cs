namespace Production.Application.Reports;

public static class ReportMessages
{
    public const int MaxMonths = 12;

    public const string CannotSee = "«Звіти цеху» доступні лише адміністратору.";
    public const string Month = "Місяць вкажіть як РРРР-ММ.";
    public const string Range = "Початок періоду не може бути пізніше кінця.";
    public const string TooLong = "Період звіту — не більше 12 місяців.";
    public const string Future = "Місяць ще не почався.";
    public const string Comparison = "Для порівняння вкажіть обидва місяці.";
}
