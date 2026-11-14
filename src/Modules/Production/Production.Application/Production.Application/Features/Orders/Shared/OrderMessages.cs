namespace Production.Application.Features.Orders.Shared;

internal static class OrderMessages
{
    public const string NotFound = "Завдання не знайдено.";
    public const string QuoteNotFound = "КП не знайдено або воно не ваше.";
    public const string QuoteNotForWorkshop = "Передати в цех можна лише КП під запит.";
    public const string QuoteItemNotFound = "Позиції або варіанту тканини немає в цьому КП.";
    public const string CannotCreate = "Створювати завдання може менеджер або адміністратор.";
    public const string CannotEdit = "Це завдання не можна змінювати.";
    public const string CannotDelete = "Видаляти завдання може лише адміністратор.";
    public const string CannotImport = "Переносити завдання може лише адміністратор.";
    public const string Stale = "Завдання вже змінили. Оновіть сторінку.";
    public const string ItemNotInOrder = "Позиції немає в цьому завданні.";
    public const string ActorUnknown = "Не вдалося визначити користувача.";
    public const string DuplicateItem = "Один варіант тканини доданий двічі.";
    public const string CannotTrack = "Етапи й відвантаження відмічає начальник цеху або адміністратор у новому завданні.";
    public const string QuantityBelowShipped = "Кількість не може бути меншою, ніж уже відвантажено.";
    public const string CannotCancel = "Скасувати можна нове завдання, яке ви можете змінювати.";
    public const string CannotReopen = "Повернути в роботу може адміністратор.";
    public const string FileNotFound = "Файл не знайдено.";
    public const string FileTooLarge = "Файл має бути не більше 20 МБ.";
    public const string FileEmpty = "Файл порожній.";
    public const string CannotSend = "Повідомлення вже надіслано або його не можна надіслати.";
    public const string TelegramNotReady = "Telegram для цеху не налаштовано: оберіть групу в «Налаштування цеху».";
    public const string EmbroideryNotFound = "Розрахунок вишивки не знайдено. Оберіть інший у «Ціна вишивки».";
    public const string SendingInProgress = "Повідомлення вже надсилається.";
}
