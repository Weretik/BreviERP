using System.Text.Json.Nodes;
using Reference.Domain.Suppliers;

namespace Reference.Infrastructure.KeepinCrm;

/// <summary>
/// Turns a KeepinCRM GET /suppliers page into snapshots. The field rules repeat the client's Google Sheet
/// script «Оновити постачальників» (custom fields shcho_zakupovuiemo_323, sait_324, kontaktnaya_osoba_297),
/// so Brevi shows the same values the sheet did.
/// </summary>
internal static class KeepinCrmSupplierParser
{
    private static readonly string[] NameKeys = ["name", "title", "company_name", "supplier_name"];

    public static IReadOnlyList<KeepinCrmSupplierSnapshot> ParsePage(JsonNode? payload)
    {
        var rows = FindSuppliersArray(payload);
        var result = new List<KeepinCrmSupplierSnapshot>(rows.Count);
        foreach (var row in rows.OfType<JsonObject>())
        {
            var id = Text(row["id"]);
            var name = FirstNonEmpty(NameKeys.Select(key => row[key]));
            if (id.Length == 0 || name.Length == 0)
                continue;

            var custom = row["custom_fields"] ?? row["customFields"];
            result.Add(new KeepinCrmSupplierSnapshot(
                id,
                name,
                Empty(CustomField(custom, "shcho_zakupovuiemo_323", "що закуповуємо", "что закупаем")),
                Empty(CustomField(custom, "sait_324", "сайт", "site", "website")),
                Empty(ContactPerson(row, custom)),
                Empty(Phones(row, custom)),
                Empty(Emails(row)),
                Empty(Tags(row))));
        }

        return result;
    }

    /// <summary>Number of supplier rows on the page, including rows skipped for a missing ID or name.</summary>
    public static int CountRows(JsonNode? payload) => FindSuppliersArray(payload).Count;

    private static JsonArray FindSuppliersArray(JsonNode? payload)
    {
        JsonNode?[] variants =
        [
            payload,
            Child(payload, "data"), Child(payload, "suppliers"), Child(payload, "items"), Child(payload, "results"),
            Child(Child(payload, "data"), "suppliers"), Child(Child(payload, "data"), "items"),
            Child(Child(payload, "data"), "results"), Child(payload, "result"),
            Child(Child(payload, "result"), "suppliers"), Child(Child(payload, "result"), "data")
        ];
        foreach (var variant in variants)
        {
            if (variant is JsonArray array && LooksLikeSuppliers(array))
                return array;
        }

        return Search(payload) ?? [];
    }

    private static JsonArray? Search(JsonNode? value) => value switch
    {
        JsonArray array when LooksLikeSuppliers(array) => array,
        JsonArray array => array.Select(Search).FirstOrDefault(found => found is not null),
        JsonObject obj => obj.Select(pair => Search(pair.Value)).FirstOrDefault(found => found is not null),
        _ => null
    };

    private static bool LooksLikeSuppliers(JsonArray array) =>
        array.Count > 0
        && array[0] is JsonObject first
        && Text(first["id"]).Length > 0
        && NameKeys.Any(key => Text(first[key]).Length > 0);

    private static JsonNode? Child(JsonNode? node, string key) => node is JsonObject obj ? obj[key] : null;

    private static string ContactPerson(JsonObject row, JsonNode? custom) => FirstNonEmpty(
    [
        JsonValue.Create(CustomField(custom, "kontaktnaya_osoba_297",
            "контактная особа", "контактна особа", "контактное лицо", "контактна людина")),
        row["contact_person"], row["contactPerson"], row["contact_name"], row["contactName"],
        Child(row["contact"], "name"), Child(row["contact"], "full_name")
    ]);

    private static string Phones(JsonObject row, JsonNode? custom)
    {
        var phones = new List<string>();
        foreach (var key in new[] { "phones", "phone", "phone_number", "mobile_phone" })
            AddValues(phones, row[key]);
        var customPhone = CustomField(custom, "phone", "телефон", "мобільний телефон", "номер телефону");
        if (customPhone.Length > 0)
            phones.Add(customPhone);
        return string.Join(", ", phones.Distinct());
    }

    private static string Emails(JsonObject row)
    {
        var emails = new List<string>();
        foreach (var key in new[] { "emails", "email", "email_address" })
            AddValues(emails, row[key]);
        return string.Join(", ", emails.Distinct());
    }

    private static string Tags(JsonObject row)
    {
        var tags = row["tag_names"] ?? row["tags"];
        if (tags is JsonValue)
            return Text(tags);
        if (tags is not JsonArray array)
            return string.Empty;
        var names = array.Select(tag => tag is JsonObject obj
                ? FirstNonEmpty([obj["name"], obj["title"], obj["value"]])
                : Text(tag))
            .Where(name => name.Length > 0);
        return string.Join(", ", names.Distinct());
    }

    private static string CustomField(JsonNode? custom, string code, params string[] names)
    {
        if (custom is JsonObject obj)
        {
            if (obj.TryGetPropertyValue(code, out var direct))
            {
                var value = Extract(direct);
                if (value.Length > 0)
                    return value;
            }

            foreach (var (key, field) in obj)
            {
                var fieldCode = FirstNonEmpty([Child(field, "code"), Child(field, "key"), Child(field, "slug")]);
                if (fieldCode.Length == 0)
                    fieldCode = key;
                if (Matches(fieldCode, code, field, names))
                {
                    var value = Extract(field);
                    if (value.Length > 0)
                        return value;
                }
            }
        }

        if (custom is JsonArray array)
        {
            foreach (var field in array)
            {
                var fieldCode = FirstNonEmpty(
                    [Child(field, "code"), Child(field, "key"), Child(field, "slug"), Child(field, "id")]);
                if (Matches(fieldCode, code, field, names))
                {
                    var value = Extract(field);
                    if (value.Length > 0)
                        return value;
                }
            }
        }

        return string.Empty;
    }

    private static bool Matches(string fieldCode, string code, JsonNode? field, string[] names)
    {
        var fieldName = Normalize(FirstNonEmpty(
            [Child(field, "name"), Child(field, "title"), Child(field, "label"), Child(field, "field_name")]));
        return Normalize(fieldCode) == Normalize(code) || names.Any(name => Normalize(name) == fieldName);
    }

    private static string Extract(JsonNode? value) => value switch
    {
        null => string.Empty,
        JsonValue => Text(value),
        JsonArray array => string.Join(", ", array.Select(Extract).Where(x => x.Length > 0).Distinct()),
        JsonObject obj => FirstNonEmpty(
            new[] { "value", "text", "content", "display_value", "displayValue" }.Select(key => obj[key]), Extract),
        _ => string.Empty
    };

    private static void AddValues(List<string> target, JsonNode? value)
    {
        switch (value)
        {
            case null:
                return;
            case JsonValue:
                var text = Text(value);
                if (text.Length > 0)
                    target.Add(text);
                return;
            case JsonArray array:
                foreach (var item in array)
                    AddValues(target, item);
                return;
            case JsonObject obj:
                foreach (var key in new[] { "value", "phone", "number", "email", "text" })
                    AddValues(target, obj[key]);
                return;
        }
    }

    private static string FirstNonEmpty(IEnumerable<JsonNode?> values, Func<JsonNode?, string>? read = null)
    {
        foreach (var value in values)
        {
            var text = (read ?? Text)(value);
            if (text.Length > 0)
                return text;
        }

        return string.Empty;
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

    private static string Normalize(string value) =>
        string.Join(' ', value.Trim().ToLowerInvariant().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    private static string? Empty(string value) => value.Length == 0 ? null : value;
}
