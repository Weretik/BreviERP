namespace Accounting.Application;

/// <summary>Messages of the module shown to the user as is.</summary>
public static class AccountingMessages
{
    public const string AccountNotFound = "Рахунок не знайдено. Оновіть список.";
    public const string CategoryNotFound = "Категорію не знайдено. Оновіть список.";
    public const string ParentNotFound = "Кореневу категорію не знайдено. Оновіть список.";
    public const string Stale = "Запис уже змінили. Відкрийте його знову, щоб побачити нові дані.";
    public const string DuplicateCategory = "Категорія з такою назвою вже є на цьому рівні.";
}
