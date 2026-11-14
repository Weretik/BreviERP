using System.Text.Json;
using System.Text.Json.Nodes;
using Crm.Application.Features.Customers.Commands.SyncKeepinCrmClients;

namespace Crm.Infrastructure.KeepinCrm;

/// <summary>
/// Turns a KeepinCRM GET /clients page (<c>{items, pagination: {total_pages}}</c>) into snapshots. The fields are those the
/// Calculator and Workshop read: id, company, person, phones, email(s), contacts[].phones/email(s), archived.
/// </summary>
internal static class KeepinCrmClientParser
{
    public static KeepinCrmClientPage ParsePage(JsonNode? payload)
    {
        var items = Child(payload, "items") as JsonArray ?? Child(payload, "data") as JsonArray ?? payload as JsonArray ?? [];
        var clients = new List<KeepinCrmClientSnapshot>(items.Count);
        foreach (var row in items.OfType<JsonObject>())
        {
            var id = Text(row["id"]);
            if (id.Length == 0)
                continue;

            var phones = new List<string>();
            var emails = new List<string>();
            AddValues(phones, row["phones"]);
            AddValues(phones, row["phone"]);
            AddValues(emails, row["emails"]);
            AddValues(emails, row["email"]);
            if (row["contacts"] is JsonArray contacts)
            {
                foreach (var contact in contacts.OfType<JsonObject>())
                {
                    AddValues(phones, contact["phones"]);
                    AddValues(phones, contact["phone"]);
                    AddValues(emails, contact["emails"]);
                    AddValues(emails, contact["email"]);
                }
            }

            clients.Add(new KeepinCrmClientSnapshot(
                id,
                Empty(Text(row["company"])),
                Empty(Text(row["person"])),
                phones.Distinct().ToList(),
                emails.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                row["archived"] is JsonValue archived && archived.GetValueKind() == JsonValueKind.True));
        }

        var totalPages = Child(Child(payload, "pagination"), "total_pages") is JsonValue total
            && total.TryGetValue<int>(out var pages) ? pages : (int?)null;
        return new KeepinCrmClientPage(clients, items.Count, totalPages);
    }

    private static JsonNode? Child(JsonNode? node, string key) => node is JsonObject obj ? obj[key] : null;

    private static void AddValues(List<string> target, JsonNode? value)
    {
        switch (value)
        {
            case JsonValue:
                foreach (var part in Text(value).Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    target.Add(part);
                return;
            case JsonArray array:
                foreach (var item in array)
                    AddValues(target, item);
                return;
            case JsonObject obj:
                foreach (var key in new[] { "value", "phone", "number", "email" })
                    AddValues(target, obj[key]);
                return;
        }
    }

    private static string Text(JsonNode? value)
    {
        if (value is not JsonValue scalar)
            return string.Empty;
        return scalar.GetValueKind() switch
        {
            JsonValueKind.String => scalar.GetValue<string>().Trim(),
            JsonValueKind.Number => scalar.ToJsonString(),
            _ => string.Empty
        };
    }

    private static string? Empty(string value) => value.Length == 0 ? null : value;
}

/// <param name="Rows">Rows on the page, including rows skipped for a missing ID.</param>
/// <param name="TotalPages">pagination.total_pages, when KeepinCRM sent it.</param>
internal sealed record KeepinCrmClientPage(IReadOnlyList<KeepinCrmClientSnapshot> Clients, int Rows, int? TotalPages);
