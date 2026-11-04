namespace Production.Application.Orders;

internal static class OrderMessages
{
    public const string NotFound = "Завдання не знайдено.";
    public const string QuoteNotFound = "КП не знайдено або воно не ваше.";
    public const string QuoteNotSent = "Передати в цех можна лише відправлене КП.";
    public const string QuoteItemNotFound = "Позиції або варіанту тканини немає в цьому КП.";
    public const string CannotCreate = "Створювати завдання може менеджер або адміністратор.";
    public const string CannotEdit = "Це завдання не можна змінювати.";
    public const string CannotDelete = "Видаляти завдання може лише адміністратор.";
    public const string CannotImport = "Переносити завдання може лише адміністратор.";
    public const string Stale = "Завдання вже змінили. Оновіть сторінку.";
    public const string ItemNotInOrder = "Позиції немає в цьому завданні.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
    public const string DuplicateItem = "Один варіант тканини доданий двічі.";
}
