using System.Text.Json;
using Crm.Application.Imports.History.Contracts;
using Crm.Infrastructure.Imports.History.Source;

namespace Crm.Infrastructure.Imports.History.Staging;

/// <summary>Recognizes only dependency fields proven by the v211 source schemas.</summary>
internal static class CrmHistoryPayloadEvidence
{
    internal static void Inspect(string entity, string? kind, string field, JsonElement source, bool required, List<CrmHistoryReference> references, HashSet<string> issues)
    {
        var raw = CrmHistoryNativeSnapshotAdapter.Text(source, field);
        if (string.IsNullOrEmpty(raw) && !required) return;
        var issue = "invalid_" + field;
        if (raw?.Contains("[truncated", StringComparison.OrdinalIgnoreCase) == true) issues.Add("truncated_json");
        try
        {
            using var document = JsonDocument.Parse(raw ?? "null");
            var payload = document.RootElement;
            if (required && payload.ValueKind != JsonValueKind.Object) throw new JsonException();
            CrmHistorySourceFiles.RequireUniqueProperties(payload);
            if (field is "before_json" or "after_json" or "changes_json") return;
            if (entity is "custom_sewing_drafts" or "custom_sewing_versions") ReadCustom(payload, references, issues, entity == "custom_sewing_versions");
            else if (entity == "quote_items" && kind is "sewing" or "ppe") issues.Add("legacy_target_reference_requires_mapping");
            else if (entity == "quote_items" && kind == "custom_sewing") ReadCustomItem(payload, references, issues);
            else if (entity == "quote_items" && kind == "one_off_ppe") ReadOneOffFile(payload, references, issues);
            else if (entity == "introduction_saved_templates") ReadIntroduction(payload, issues);
            else if (entity == "quote_versions" && (!payload.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)) issues.Add("unsupported_payload_shape");
        }
        catch (Exception error) when (error is JsonException or InvalidDataException) { issues.Add(issue); }
    }

    private static void ReadCustom(JsonElement payload, List<CrmHistoryReference> references, HashSet<string> issues, bool requireSnapshot = true)
    {
        if (requireSnapshot && (CrmHistoryNativeSnapshotAdapter.Text(payload, "type") != "custom_sewing" ||
            !payload.TryGetProperty("input", out var snapshotInput) || snapshotInput.ValueKind != JsonValueKind.Object ||
            !payload.TryGetProperty("lines", out var lines) || lines.ValueKind != JsonValueKind.Array ||
            !payload.TryGetProperty("prices", out var prices) || prices.ValueKind != JsonValueKind.Object ||
            !payload.TryGetProperty("cost", out var cost) || cost.ValueKind != JsonValueKind.Number)) issues.Add("unsupported_payload_shape");
        var input = payload.TryGetProperty("input", out var nested) && nested.ValueKind == JsonValueKind.Object ? nested : payload;
        if (!input.TryGetProperty("files", out var files) || files.ValueKind != JsonValueKind.Array || files.GetArrayLength() > 30)
        { issues.Add("unsupported_payload_shape"); return; }
        foreach (var file in files.EnumerateArray())
        {
            var id = CrmHistoryNativeSnapshotAdapter.Text(file, "id");
            if (id is null) issues.Add("unsupported_payload_shape");
            else CrmHistoryRowNormalizer.AddReference(references, "custom_sewing_files", id, issues);
        }
        if (input.TryGetProperty("applications", out var applications))
        {
            if (applications.ValueKind != JsonValueKind.Array || applications.GetArrayLength() > 20) { issues.Add("unsupported_payload_shape"); return; }
            foreach (var application in applications.EnumerateArray())
            {
                if (application.ValueKind != JsonValueKind.Object) { issues.Add("unsupported_payload_shape"); continue; }
                if (application.TryGetProperty("artworkId", out var rawArtwork) && rawArtwork.ValueKind is not (JsonValueKind.String or JsonValueKind.Null)) { issues.Add("unsupported_payload_shape"); continue; }
                var id = CrmHistoryNativeSnapshotAdapter.Text(application, "artworkId");
                if (id is not null) CrmHistoryRowNormalizer.AddReference(references, "custom_sewing_files", id, issues);
            }
        }
    }

    private static void ReadCustomItem(JsonElement payload, List<CrmHistoryReference> references, HashSet<string> issues)
    {
        var id = CrmHistoryNativeSnapshotAdapter.Text(payload, "customVersionId");
        if (id is null) issues.Add("unsupported_payload_shape");
        else CrmHistoryRowNormalizer.AddReference(references, "custom_sewing_versions", id, issues);
        if (payload.TryGetProperty("custom", out var custom) && custom.ValueKind == JsonValueKind.Object) ReadCustom(custom, references, issues);
        if (!payload.TryGetProperty("customFabricVariants", out var variants)) return;
        if (variants.ValueKind != JsonValueKind.Array || variants.GetArrayLength() > 100) { issues.Add("unsupported_payload_shape"); return; }
        foreach (var variant in variants.EnumerateArray())
        {
            var version = CrmHistoryNativeSnapshotAdapter.Text(variant, "customVersionId");
            if (version is null) issues.Add("unsupported_payload_shape");
            else CrmHistoryRowNormalizer.AddReference(references, "custom_sewing_versions", version, issues);
            if (variant.ValueKind == JsonValueKind.Object && variant.TryGetProperty("custom", out var model) && model.ValueKind == JsonValueKind.Object) ReadCustom(model, references, issues);
        }
    }

    private static void ReadOneOffFile(JsonElement payload, List<CrmHistoryReference> references, HashSet<string> issues)
    {
        var sourceId = CrmHistoryNativeSnapshotAdapter.Text(payload, "id");
        if (CrmHistoryNativeSnapshotAdapter.Text(payload, "kind") != "one_off_ppe" || sourceId?.StartsWith("one-off:", StringComparison.Ordinal) != true ||
            string.IsNullOrWhiteSpace(CrmHistoryNativeSnapshotAdapter.Text(payload, "name")) ||
            !payload.TryGetProperty("prices", out var prices) || prices.ValueKind != JsonValueKind.Object ||
            !HasPrice(prices, "retail", true) || !HasPrice(prices, "wholesale", false) || !HasPrice(prices, "large", false) ||
            !payload.TryGetProperty("shown", out var shown) || shown.ValueKind != JsonValueKind.Array || shown.GetArrayLength() != 3 ||
            shown.EnumerateArray().Any(x => x.ValueKind is not (JsonValueKind.True or JsonValueKind.False))) issues.Add("unsupported_payload_shape");
        var photo = CrmHistoryNativeSnapshotAdapter.Text(payload, "photo");
        if (string.IsNullOrEmpty(photo)) return;
        const string prefix = "/api/custom-sewing/files/";
        if (!photo.StartsWith(prefix, StringComparison.Ordinal)) issues.Add("unsupported_payload_shape");
        else CrmHistoryRowNormalizer.AddReference(references, "custom_sewing_files", photo[prefix.Length..], issues);
    }

    private static bool HasPrice(JsonElement prices, string tier, bool positive) => prices.TryGetProperty(tier, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var amount) && (positive ? amount > 0 : amount >= 0);

    private static void ReadIntroduction(JsonElement payload, HashSet<string> issues)
    {
        var template = CrmHistoryNativeSnapshotAdapter.Text(payload, "template");
        var email = CrmHistoryNativeSnapshotAdapter.Text(payload, "emailTemplateId");
        if (template is not ("general" or "summer" or "winter") || string.IsNullOrEmpty(email)
            || !payload.TryGetProperty("selections", out var selections) || selections.ValueKind != JsonValueKind.Array
            || selections.GetArrayLength() != (template == "general" ? 8 : 4))
        { issues.Add("unsupported_payload_shape"); return; }
        foreach (var selection in selections.EnumerateArray())
        {
            if (CrmHistoryNativeSnapshotAdapter.OptionalNumber(selection, "uid") is null ||
                string.IsNullOrEmpty(CrmHistoryNativeSnapshotAdapter.Text(selection, "productId")) || string.IsNullOrEmpty(CrmHistoryNativeSnapshotAdapter.Text(selection, "fabricId")))
            { issues.Add("unsupported_payload_shape"); return; }
        }
        // Legacy Product/Fabric/email-template references are not target IDs; reviewed mappings are still required.
        issues.Add("legacy_target_reference_requires_mapping");
    }
}
