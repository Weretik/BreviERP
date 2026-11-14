using System.Text.Json;

namespace Purchasing.Application.Features.Register.Queries.GetPurchaseInvoices.Results;

/// <summary>Cells kept only in the raw row text (chat name, author) and shown on the card.</summary>
internal sealed record PurchaseRawCells(string? ChatName, string? FromUser)
{
    public static PurchaseRawCells Read(string rawJson)
    {
        try
        {
            using var document = JsonDocument.Parse(rawJson);
            return new PurchaseRawCells(Cell(document.RootElement, "ChatName"), Cell(document.RootElement, "FromUser"));
        }
        catch (JsonException)
        {
            return new PurchaseRawCells(null, null);
        }
    }

    private static string? Cell(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
        && value.GetString() is { } text && text.Trim().Length > 0
            ? text
            : null;
}
