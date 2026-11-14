using System.Globalization;
using BuildingBlocks.Domain.Results;
using Purchasing.Domain.Messages;
using Purchasing.Domain.Ingestion.Errors;

namespace Purchasing.Domain.Ingestion.Normalization;

/// <summary>Turns one sheet row into a <see cref="NormalizedSourceRow"/>; fully empty rows are skipped (null).</summary>
public static class SourceRowNormalizer
{
    private static readonly CultureInfo Ukrainian = CultureInfo.GetCultureInfo("uk-UA");

    public const string PaidMark = "СПЛАЧЕНО";

    public static DomainResult<NormalizedSourceRow?> Normalize(IReadOnlyList<string?> row, int rowNumber, string spreadsheetId, long sheetId)
    {
        if (rowNumber < 2) return PurchaseSourceErrors.RowNumberInvalid(rowNumber);
        if (row.Count > PurchaseSheetColumns.MaxRowLength)
            return PurchaseSourceErrors.TooManyColumns(rowNumber, row.Count);

        var cells = new string[PurchaseSheetColumns.Header.Count];
        for (var i = 0; i < cells.Length; i++) cells[i] = i < row.Count ? row[i] ?? "" : "";
        if (cells.All(x => x.Trim().Length == 0)) return DomainResult<NormalizedSourceRow?>.Success(null);

        var uid = cells[PurchaseSheetColumns.Uid].Trim();
        var chatId = cells[PurchaseSheetColumns.ChatId].Trim();
        var telegramMessageId = cells[PurchaseSheetColumns.MessageId].Trim();
        if (uid.Length == 0 || chatId.Length == 0 || telegramMessageId.Length == 0)
            return PurchaseSourceErrors.IdentityMissing(rowNumber);

        var telegramKey = PurchaseIdentity.TelegramKey(chatId, telegramMessageId);
        var messageId = PurchaseIdentity.MessageId(telegramKey);
        var raw = JsJson.Object(PurchaseSheetColumns.Header.Select((name, i) => KeyValuePair.Create(name, cells[i])).ToList());
        var links = FileLinkRules.Extract(cells[PurchaseSheetColumns.FileLinks]);
        var issues = FileLinkRules.Issues(cells[PurchaseSheetColumns.FileLinks], cells[PurchaseSheetColumns.FileCount], links);

        var status = cells[PurchaseSheetColumns.Status];
        var message = new MessageSource(
            Optional(cells[PurchaseSheetColumns.DateTime]),
            MessageDates.Parse(cells[PurchaseSheetColumns.DateTime]),
            cells[PurchaseSheetColumns.Text],
            status,
            Optional(cells[PurchaseSheetColumns.StatusDate]),
            Optional(cells[PurchaseSheetColumns.FromUserStatus]),
            status.Trim().ToUpper(Ukrainian) == PaidMark ? PaymentState.MarkedPaid : PaymentState.Unconfirmed);

        return (NormalizedSourceRow?)new NormalizedSourceRow(PurchaseIdentity.SourceId(spreadsheetId, sheetId, uid), messageId, telegramKey, uid,
            rowNumber, raw, links, issues, message);
    }

    private static string? Optional(string value) => value.Trim().Length == 0 ? null : value;
}
