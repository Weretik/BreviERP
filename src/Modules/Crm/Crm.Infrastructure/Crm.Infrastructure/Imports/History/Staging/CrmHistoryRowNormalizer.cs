using System.Text.Json;
using System.Text.RegularExpressions;
using Crm.Application.Imports.History.Contracts;
using Crm.Infrastructure.Imports.History.Source;

namespace Crm.Infrastructure.Imports.History.Staging;

internal static partial class CrmHistoryRowNormalizer
{
    [GeneratedRegex("^[A-Za-z0-9_-]{1,128}$", RegexOptions.CultureInvariant)] private static partial Regex SourceIdPattern();
    internal static bool IsSourceId(string? value) => value is not null && SourceIdPattern().IsMatch(value);

    internal static CrmHistoryRow Normalize(string entity, JsonElement source, IReadOnlyDictionary<string, CrmHistoryBlobProof> proofs)
    {
        var hash = CrmHistorySourceFiles.HashText(source.GetRawText());
        var issues = new HashSet<string>(StringComparer.Ordinal);
        var id = CrmHistoryNativeSnapshotAdapter.Text(source, "id");
        if (!IsSourceId(id)) { id = "invalid_" + hash; issues.Add("invalid_source_id"); }
        if (source.ValueKind != JsonValueKind.Object) return new(entity, id!, hash, [], [], [], [], null, ["invalid_source_id", "invalid_row_shape"]);
        var roles = entity == "counterparties" ? ReadRoles(source, issues) : [];
        var owners = ReadOwners(entity, id!, source, issues);
        var references = new List<CrmHistoryReference>();
        void Parent(string field, string target, bool required = true)
        {
            var raw = CrmHistoryNativeSnapshotAdapter.Text(source, field);
            if (string.IsNullOrEmpty(raw)) { if (required) issues.Add("missing_reference"); return; }
            AddReference(references, target, raw, issues);
        }
        switch (entity)
        {
            case "quotes":
                Parent("counterparty_id", "counterparties", false);
                if (string.IsNullOrEmpty(CrmHistoryNativeSnapshotAdapter.Text(source, "counterparty_id"))) issues.Add("unresolved_recipient");
                break;
            case "quote_versions": Parent("quote_id", "quotes"); Parent("parent_version_id", "quote_versions", false); break;
            case "quote_items": Parent("quote_version_id", "quote_versions"); break;
            case "quote_change_log": Parent("quote_id", "quotes"); Parent("quote_version_id", "quote_versions"); Parent("quote_item_id", "quote_items", false); break;
            case "quote_transfers": Parent("quote_id", "quotes"); break;
            case "quote_email_sends": Parent("quote_id", "quotes"); Parent("quote_version_id", "quote_versions"); break;
            case "quote_ppe_price_overrides": Parent("quote_item_id", "quote_items"); break;
        }
        var kind = entity == "quote_items" ? CrmHistoryNativeSnapshotAdapter.Text(source, "item_kind") : null;
        if (entity == "quote_items" && kind is not ("sewing" or "ppe" or "one_off_ppe" or "custom_sewing" or "individual")) kind = "unknown";
        var jsonFields = entity switch
        {
            "quote_items" or "introduction_saved_templates" => new[] { ("snapshot_json", true) },
            "quote_versions" => [("snapshot_json", true), ("changes_json", false)],
            "quote_change_log" => [("before_json", false), ("after_json", false)],
            "custom_sewing_drafts" or "custom_sewing_versions" => [("payload_json", true)],
            _ => []
        };
        foreach (var (field, required) in jsonFields)
            CrmHistoryPayloadEvidence.Inspect(entity, kind, field, source, required, references, issues);
        if (entity == "custom_sewing_files") CheckFile(id!, source, proofs, issues);
        var external = new List<string>();
        if (entity == "counterparties")
        {
            var tuple = new[] { CrmHistoryNativeSnapshotAdapter.Text(source, "external_source"), CrmHistoryNativeSnapshotAdapter.Text(source, "external_type"), CrmHistoryNativeSnapshotAdapter.Text(source, "external_id") };
            if (tuple.All(x => !string.IsNullOrEmpty(x))) external.Add(CrmHistorySourceFiles.HashText(JsonSerializer.Serialize(tuple)));
        }
        return new(entity, id!, hash, roles, owners, references.Distinct().ToArray(), external, kind, issues.Order(StringComparer.Ordinal).ToArray());
    }

    private static IReadOnlyList<string> ReadRoles(JsonElement row, HashSet<string> issues)
    {
        var raw = CrmHistoryNativeSnapshotAdapter.Text(row, "roles_json");
        try
        {
            using var document = JsonDocument.Parse(raw ?? "null");
            if (document.RootElement.ValueKind != JsonValueKind.Array || document.RootElement.GetArrayLength() > 20) throw new JsonException();
            var roles = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in document.RootElement.EnumerateArray())
            {
                if (value.ValueKind != JsonValueKind.String) throw new JsonException();
                var role = value.GetString(); roles.Add(role is "CLIENT" or "SUPPLIER" or "FINANCIAL" ? role : "UNKNOWN");
            }
            return roles.Order(StringComparer.Ordinal).ToArray();
        }
        catch (JsonException) { issues.Add("invalid_roles_json"); return []; }
    }

    private static IReadOnlyList<string> ReadOwners(string entity, string id, JsonElement row, HashSet<string> issues)
    {
        if (entity == "counterparties") return [CrmHistorySourceFiles.HashText("counterparty:" + id)];
        string[] fields = entity switch
        {
            "quotes" => ["created_by_email", "current_responsible_email"],
            "quote_versions" or "introduction_saved_templates" => ["created_by_email"],
            "quote_change_log" or "quote_ppe_price_overrides" => ["changed_by_email"],
            "quote_transfers" => ["from_email", "to_email"],
            "quote_email_sends" => ["sent_by_email"],
            "custom_sewing_drafts" or "custom_sewing_versions" or "custom_sewing_files" => ["owner_email"],
            _ => []
        };
        var owners = new HashSet<string>(StringComparer.Ordinal);
        foreach (var field in fields)
        {
            var value = CrmHistoryNativeSnapshotAdapter.Text(row, field);
            if (string.IsNullOrWhiteSpace(value)) { issues.Add("missing_owner_identity"); continue; }
            owners.Add(CrmHistorySourceFiles.HashText(value.Trim().ToLowerInvariant()));
        }
        return owners.Order(StringComparer.Ordinal).ToArray();
    }

    internal static void AddReference(List<CrmHistoryReference> references, string entity, string value, HashSet<string> issues)
    {
        if (!IsSourceId(value)) { issues.Add("invalid_source_id"); return; }
        var reference = new CrmHistoryReference(entity,value);
        if (references.Contains(reference)) return;
        if (references.Count >= CrmHistoryLimits.MaximumRowReferences) { issues.Add("graph_limit_exceeded"); return; }
        references.Add(reference);
    }

    private static void CheckFile(string id, JsonElement row, IReadOnlyDictionary<string, CrmHistoryBlobProof> proofs, HashSet<string> issues)
    {
        var validLength = row.TryGetProperty("size", out var size) && size.ValueKind == JsonValueKind.Number && size.TryGetInt64(out var number) && number > 0;
        var mime = CrmHistoryNativeSnapshotAdapter.Text(row, "mime_type");
        if (!validLength || string.IsNullOrEmpty(mime) || mime.Length > 127 || mime.Any(x => x <= 32 || x >= 127) || !mime.Contains('/')) issues.Add("invalid_file_metadata");
        if (!proofs.TryGetValue(id, out var proof)) { issues.Add("missing_file_bytes"); issues.Add("missing_file_hash"); }
        else if (!proof.Verified || !validLength || size.GetInt64() != proof.ActualLength) issues.Add("invalid_file_metadata");
    }
}
