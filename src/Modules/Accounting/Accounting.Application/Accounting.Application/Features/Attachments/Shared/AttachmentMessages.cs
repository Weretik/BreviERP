namespace Accounting.Application.Features.Attachments.Shared;

internal static class AttachmentMessages
{
    public const string StorageNotConfigured =
        "Сховище файлів бухгалтерії ще не налаштоване. Файли можна буде додати після налаштування.";
    public const string StorageUnavailable = "Сховище файлів зараз недоступне. Спробуйте пізніше.";
    public const string NotFound = "Файл не знайдено. Оновіть список.";
    public const string Missing = "Файл є в списку, але його немає у сховищі.";
    public const string Empty = "Файл порожній.";
    public const string TooLarge = "Файл більший за 10 МБ.";
    public const string WrongType = "Підходять лише PDF, JPG або PNG.";
    public const string TransactionVoided = "Операцію анульовано: файли до неї не додаються.";
}
