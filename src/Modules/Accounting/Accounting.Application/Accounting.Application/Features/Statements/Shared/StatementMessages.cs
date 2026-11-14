namespace Accounting.Application.Features.Statements.Shared;

/// <summary>Messages of «Виписки банку» shown to the director as is.</summary>
internal static class StatementMessages
{
    public const string WrongType = "Оберіть виписку у форматі XLSX, XLS або CSV.";
    public const string Empty = "Файл порожній.";
    public const string TooLarge = "Файл більший за 12 МБ. Розділіть виписку на частини.";
    public const string NotFound = "Виписку не знайдено. Оновіть список.";
    public const string LineNotFound = "Рядок виписки не знайдено. Оновіть виписку.";
    public const string TransferAccountNotFound = "Рахунок переказу не знайдено. Оновіть виписку.";
}
