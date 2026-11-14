using System.Globalization;
using System.Text.Json;
using Accounting.Application.Abstractions;
using Accounting.Domain.Audit;
using Accounting.Application.Features.Journal.Shared;
using Accounting.Application.Features.Reports.Shared;

namespace Accounting.Application.Features.Audit.Queries.GetFinanceAudit;

/// <summary>A short name of the record from its snapshot.</summary>
internal static class AuditTitle
{
    public static string Of(string? snapshot)
    {
        if (string.IsNullOrWhiteSpace(snapshot)) return string.Empty;
        try
        {
            using var document = JsonDocument.Parse(snapshot);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) return string.Empty;
            var name = Text(root, "name") ?? Text(root, "fileName");
            if (name is not null) return name;

            var parts = new List<string>();
            if (Text(root, "date") is { } date && DateOnly.TryParse(date, CultureInfo.InvariantCulture, out var day))
                parts.Add(day.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture));
            if (root.TryGetProperty("amountMinor", out var amount) && amount.TryGetInt64(out var minor))
                parts.Add($"{FinanceReportCsv.Money(minor)} {Text(root, "currency") ?? string.Empty}".TrimEnd());
            if (Text(root, "fromAccountName") is { } from && Text(root, "toAccountName") is { } to)
                parts.Add($"{from} → {to}");
            if ((Text(root, "description") ?? Text(root, "note")) is { } description) parts.Add(description);
            return string.Join(" · ", parts);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static string? Text(JsonElement root, string property) =>
        root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            && value.GetString() is { Length: > 0 } text
            ? text
            : null;
}
