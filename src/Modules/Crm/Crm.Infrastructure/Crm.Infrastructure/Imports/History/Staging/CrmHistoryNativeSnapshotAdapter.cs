using System.Text.Json;
using Crm.Application.Imports.History.Contracts;
using Crm.Infrastructure.Imports.History.Source;

namespace Crm.Infrastructure.Imports.History.Staging;

internal static class CrmHistoryNativeSnapshotAdapter
{
    private static readonly string[] Entities = ["counterparties", "quotes", "quote_versions", "quote_items", "quote_change_log", "quote_transfers", "quote_email_sends", "quote_ppe_price_overrides", "custom_sewing_drafts", "custom_sewing_versions", "custom_sewing_files", "introduction_saved_templates"];

    internal static CrmHistorySnapshot Read(string datasetId, string databaseHash, string manifestHash, JsonElement database, JsonElement manifest, IReadOnlyDictionary<string, CrmHistoryBlobProof> proofs)
    {
        if (database.ValueKind != JsonValueKind.Object || manifest.ValueKind != JsonValueKind.Object ||
            !manifest.TryGetProperty("tables", out var evidence) || evidence.ValueKind != JsonValueKind.Object)
            throw CrmHistorySourceFiles.Invalid();
        EnsureUniqueNames(database); EnsureUniqueNames(manifest); EnsureUniqueNames(evidence);
        // Reject row amplification before constructing any normalized metadata.
        long selectedRows = 0;
        foreach (var entity in Entities)
        {
            if (!database.TryGetProperty(entity, out var table)) continue;
            if (table.ValueKind != JsonValueKind.Object || !table.TryGetProperty("rows", out var candidateRows) || candidateRows.ValueKind != JsonValueKind.Array)
                throw CrmHistorySourceFiles.Invalid();
            selectedRows += candidateRows.GetArrayLength();
            if (selectedRows > CrmHistoryLimits.MaximumRows) throw CrmHistorySourceFiles.Invalid();
        }
        var normalizedReferences = 0;
        var rows = new List<CrmHistorySourceRow>();
        var tables = new List<CrmHistoryTable>();
        foreach (var entity in Entities)
        {
            if (!database.TryGetProperty(entity, out var source) || !evidence.TryGetProperty(entity, out var tableEvidence))
            {
                if (database.TryGetProperty(entity, out _) || (evidence.TryGetProperty(entity, out var missingEvidence) && Number(missingEvidence, "rows_exported") != 0))
                    throw CrmHistorySourceFiles.Invalid();
                tables.Add(new CrmHistoryTable(entity, 0, 0, false, ["source_table_missing"]));
                continue;
            }
            if (source.ValueKind != JsonValueKind.Object || tableEvidence.ValueKind != JsonValueKind.Object) throw CrmHistorySourceFiles.Invalid();
            EnsureUniqueNames(source); EnsureUniqueNames(tableEvidence);
            if (!source.TryGetProperty("columns", out var columns) || columns.ValueKind != JsonValueKind.Array || columns.EnumerateArray().Any(x => x.ValueKind != JsonValueKind.String)
                || !source.TryGetProperty("rows", out var sourceRows) || sourceRows.ValueKind != JsonValueKind.Array)
                throw CrmHistorySourceFiles.Invalid();
            var expected = Number(tableEvidence, "rows_exported");
            if (expected != sourceRows.GetArrayLength()) throw CrmHistorySourceFiles.Invalid();
            var issues = new List<string>();
            if (Text(tableEvidence, "status") != "all_available_rows_read") issues.Add("source_table_incomplete");
            if (!Flag(tableEvidence, "reached_last_page")) issues.Add("source_last_page_unverified");
            if (ArrayCount(tableEvidence, "truncated_pages") > 0) issues.Add("source_truncated_pages");
            if (ArrayCount(tableEvidence, "issues") > 0) issues.Add("source_read_issues");
            if (ArrayCount(tableEvidence, "redacted_fields") > 0) issues.Add("source_fields_redacted");
            if (!Flag(tableEvidence, "columns_match_source_schema")) issues.Add("source_schema_unverified");
            tables.Add(new CrmHistoryTable(entity, expected, sourceRows.GetArrayLength(), issues.Count == 0, issues));
            foreach (var row in sourceRows.EnumerateArray())
            {
                if (row.ValueKind == JsonValueKind.Object) EnsureUniqueNames(row);
                var metadata = CrmHistoryRowNormalizer.Normalize(entity, row, proofs);
                if (metadata.References.Count > CrmHistoryLimits.MaximumReferences - normalizedReferences) throw CrmHistorySourceFiles.Invalid();
                normalizedReferences += metadata.References.Count;
                rows.Add(new CrmHistorySourceRow(metadata, row));
            }
        }
        var fileIds = rows.Where(x => x.Metadata.Entity == "custom_sewing_files").Select(x => x.Metadata.SourceId).ToHashSet(StringComparer.Ordinal);
        if (proofs.Keys.Any(x => !fileIds.Contains(x))) throw CrmHistorySourceFiles.Invalid();
        var normalized = CrmHistoryGraphEvidence.Apply(rows);
        return new CrmHistorySnapshot(datasetId, databaseHash, manifestHash, Flag(manifest, "full_backup"), Flag(manifest, "transaction_consistent_snapshot"), tables, normalized, []);
    }

    internal static string? Text(JsonElement row, string field) => row.ValueKind == JsonValueKind.Object && row.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
    internal static int? OptionalNumber(JsonElement row, string field) => row.ValueKind == JsonValueKind.Object && row.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) ? number : null;
    private static int Number(JsonElement row, string field) => row.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number) && number >= 0 ? number : throw CrmHistorySourceFiles.Invalid();
    private static bool Flag(JsonElement row, string field) => row.TryGetProperty(field, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False ? value.GetBoolean() : throw CrmHistorySourceFiles.Invalid();
    private static int ArrayCount(JsonElement row, string field) => row.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.Array ? value.GetArrayLength() : throw CrmHistorySourceFiles.Invalid();
    private static void EnsureUniqueNames(JsonElement row)
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var property in row.EnumerateObject()) if (!names.Add(property.Name)) throw CrmHistorySourceFiles.Invalid();
    }
}
