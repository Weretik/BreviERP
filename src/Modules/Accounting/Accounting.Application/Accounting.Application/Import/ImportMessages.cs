namespace Accounting.Application.Import;

/// <summary>Messages of «Імпорт архіву Фінансів», shown to the user as is.</summary>
internal static class ImportMessages
{
    public const string Empty = "Оберіть файл finance.json з експорту Калькулятора.";
    public const string NotJson = "Файл не схожий на експорт «Фінансів» Калькулятора (finance.json).";
    public const string BrokenArchive = "Стиснений файл пошкоджено. Оберіть finance.json ще раз.";
    public const string TooLarge = "Файл завеликий для імпорту.";
    public const string WrongFormat = "Інша версія експорту: очікується «brevi-finance-v5», у файлі — «{0}».";
    public const string Incomplete = "Експорт неповний (complete ≠ true). Зробіть експорт у Калькуляторі ще раз.";
    public const string WrongCutover = "Дата переходу у файлі — {0}, а в Brevi — 01.10.2026. Такий архів не підходить.";
    public const string FilesSkipped = "Файлів операцій у експорті: {0}. Їх не переносимо — додайте потрібні вручну.";
    public const string Stale = "Поки йшов імпорт, дані «Бухгалтерії» змінились. Запустіть перевірку ще раз.";
}
