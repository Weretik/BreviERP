using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Purchasing.Domain.Ingestion;

namespace Purchasing.Domain.Documents;

/// <summary>
/// Canonical JSON of a checked document: fixed field order, no unknown fields, Cyrillic as is. Its SHA-256 is the
/// document hash, so saving the same facts twice changes nothing.
/// </summary>
public static class PurchaseDocumentJson
{
    private static readonly JsonWriterOptions Options = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public static string Write(PurchaseDocument document)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream, Options))
        {
            writer.WriteStartObject();
            writer.WriteString("sourceUID", document.SourceUid);
            writer.WriteString("originalFileLink", document.OriginalFileLink);
            writer.WriteString("documentType", document.DocumentType);
            writer.WriteString("documentNumber", document.DocumentNumber);
            writer.WriteString("documentDate", document.DocumentDate);
            writer.WriteString("supplierName", document.SupplierName);
            writer.WriteString("vat", PurchaseDocumentRules.VatName(document.Vat));
            writer.WriteBoolean("pricesIncludeVat", document.PricesIncludeVat);
            writer.WriteStartArray("items");
            foreach (var item in document.Items)
            {
                writer.WriteStartObject();
                writer.WriteNumber("position", item.Position);
                writer.WriteString("name", item.Name);
                writer.WriteString("quantity", item.Quantity);
                if (item.Unit is null) writer.WriteNull("unit");
                else writer.WriteString("unit", item.Unit);
                writer.WriteString("price", item.Price);
                writer.WriteString("sum", item.Sum);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            if (document.VatAmount is null) writer.WriteNull("vatAmount");
            else writer.WriteString("vatAmount", document.VatAmount);
            writer.WriteString("totalPayable", document.TotalPayable);
            writer.WriteEndObject();
        }

        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string Hash(string payloadJson) => PurchaseIdentity.Sha256(payloadJson);

    /// <summary>
    /// Reads a stored payload back; it was checked when saved. Null when it no longer passes the rules or is not the
    /// expected JSON (rules changed since, or the row was edited by hand); the caller reports it as a problem.
    /// </summary>
    public static PurchaseDocument? Read(string payloadJson)
    {
        try
        {
            using var json = JsonDocument.Parse(payloadJson);
            var root = json.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("sourceUID", out var source) || source.ValueKind != JsonValueKind.String
                || !root.TryGetProperty("originalFileLink", out var link) || link.ValueKind != JsonValueKind.String)
                return null;
            return PurchaseDocumentRules.Read(root, source.GetString()!, link.GetString()!, identityInJson: false).Document;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
