using System.Security.Cryptography;
using System.Text;

namespace Purchasing.Domain.Ingestion;

/// <summary>
/// Deterministic ids of the purchase register: lower-case hex SHA-256 of UTF-8 text. The same formulas as the old
/// Calculator, so its saved ids, supplier links and checked documents stay valid after a transfer.
/// </summary>
public static class PurchaseIdentity
{
    public static string TelegramKey(string chatId, string messageId) => $"{chatId.Trim()}:{messageId.Trim()}";

    public static string MessageId(string telegramKey) => Sha256("telegram:" + telegramKey);

    public static string SourceId(string spreadsheetId, long sheetId, string uid) =>
        Sha256($"{spreadsheetId}:{sheetId.ToString(System.Globalization.CultureInfo.InvariantCulture)}:{uid.Trim()}");

    public static string FileId(string messageId, string exactUrl) => Sha256($"{messageId}:{exactUrl}");

    public static string RevisionId(string sourceId, string rawJson) => Sha256($"{sourceId}:{rawJson}");

    public static string Sha256(string text) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}
