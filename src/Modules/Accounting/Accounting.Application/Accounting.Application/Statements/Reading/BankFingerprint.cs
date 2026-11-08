using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Accounting.Domain.Categories;

namespace Accounting.Application.Statements.Reading;

/// <summary>
/// Identity of a statement line, the same as the old Calculator computed it, so lines it imported (moved in stage 5)
/// are recognised: SHA-256 (lowercase hex) of the compact JSON array
/// <c>[accountId, date, time, amountMinor, currency, type, iban, edrpou, document, bankId, lowercase(trim(purpose))]</c>.
/// </summary>
internal static class BankFingerprint
{
    private static readonly JsonWriterOptions Options = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public static string Of(Guid accountId, DateOnly date, TimeOnly? time, long amountMinor, string currency,
        CategoryDirection type, string iban, string edrpou, string document, string? bankId, string purpose)
    {
        using var buffer = new MemoryStream();
        using (var json = new Utf8JsonWriter(buffer, Options))
        {
            json.WriteStartArray();
            json.WriteStringValue(accountId.ToString("D"));
            json.WriteStringValue(date.ToString("yyyy-MM-dd"));
            if (time is { } value) json.WriteStringValue(value.ToString("HH:mm:ss"));
            else json.WriteNullValue();
            json.WriteNumberValue(amountMinor);
            json.WriteStringValue(currency);
            json.WriteStringValue(type == CategoryDirection.Income ? "INCOME" : "EXPENSE");
            json.WriteStringValue(iban);
            json.WriteStringValue(edrpou);
            json.WriteStringValue(document);
            if (bankId is null) json.WriteNullValue();
            else json.WriteStringValue(bankId);
            json.WriteStringValue(purpose.Trim().ToLowerInvariant());
            json.WriteEndArray();
        }

        return Convert.ToHexStringLower(SHA256.HashData(buffer.ToArray()));
    }
}
