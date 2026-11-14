using BuildingBlocks.Domain.Errors;

namespace Purchasing.Domain.Ingestion.Errors;

/// <summary>Rules of the purchase sheet read as a whole; the sync records the message and stops.</summary>
public static class PurchaseSourceErrors
{
    public static Error RowNumberInvalid(int rowNumber) =>
        Error.Validation("Purchasing.Source.RowNumber", $"Неправильний номер рядка {rowNumber}.");

    public static Error TooManyColumns(int rowNumber, int count) =>
        Error.Validation("Purchasing.Source.Columns", $"Рядок {rowNumber}: забагато стовпців ({count}).");

    public static Error IdentityMissing(int rowNumber) =>
        Error.Validation("Purchasing.Source.Identity", $"Рядок {rowNumber}: немає UID, ChatId або MessageId.");

    public static Error UidRepeated(string uid, IEnumerable<int> rows) =>
        Error.Validation("Purchasing.Source.Uid", $"UID {uid} повторюється в рядках {string.Join(", ", rows)}.");

    public static Error MarksDiffer(string telegramKey, IEnumerable<int> rows) =>
        Error.Validation("Purchasing.Source.Marks",
            $"Повідомлення {telegramKey}: різні Status/StatusDate/FromUserStatus у рядках {string.Join(", ", rows)}.");
}
