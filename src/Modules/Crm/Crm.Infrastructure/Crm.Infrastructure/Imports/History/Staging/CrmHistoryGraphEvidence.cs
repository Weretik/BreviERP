using System.Text.Json;
using Crm.Application.Imports.History.Contracts;
using Crm.Infrastructure.Imports.History.Source;

namespace Crm.Infrastructure.Imports.History.Staging;

internal static class CrmHistoryGraphEvidence
{
    internal static IReadOnlyList<CrmHistoryRow> Apply(IReadOnlyList<CrmHistorySourceRow> source)
    {
        if (source.Count > CrmHistoryLimits.MaximumRows) throw CrmHistorySourceFiles.Invalid();
        var versions = source.Where(x => x.Metadata.Entity == "quote_versions")
            .ToLookup(x => CrmHistoryNativeSnapshotAdapter.Text(x.Source, "quote_id") ?? "", StringComparer.Ordinal);
        var items = source.Where(x => x.Metadata.Entity == "quote_items")
            .ToLookup(x => CrmHistoryNativeSnapshotAdapter.Text(x.Source, "quote_version_id") ?? "", StringComparer.Ordinal);
        var itemCounts = items.ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal);
        var versionGroups = new Dictionary<(string QuoteId, int Number), (CrmHistorySourceRow First, int Count)>();
        foreach (var version in source.Where(x => x.Metadata.Entity == "quote_versions"))
        {
            var number = CrmHistoryNativeSnapshotAdapter.OptionalNumber(version.Source, "version_number");
            if (number is not > 0) continue;
            var key = (CrmHistoryNativeSnapshotAdapter.Text(version.Source, "quote_id") ?? "", number.Value);
            versionGroups[key] = versionGroups.TryGetValue(key, out var previous) ? (previous.First, previous.Count + 1) : (version, 1);
        }
        var ambiguousQuotes = versionGroups.Where(x => x.Value.Count > 1).Select(x => x.Key.QuoteId).ToHashSet(StringComparer.Ordinal);
        var result = new List<CrmHistoryRow>(source.Count);
        var totalReferences = 0;
        foreach (var row in source)
        {
            var issues = row.Metadata.Issues.ToHashSet(StringComparer.Ordinal);
            var references = new List<CrmHistoryReference>();
            var knownReferences = new HashSet<CrmHistoryReference>();
            void AddReference(CrmHistoryReference reference)
            {
                if (knownReferences.Contains(reference)) return;
                if (references.Count >= CrmHistoryLimits.MaximumRowReferences) { issues.Add("graph_limit_exceeded"); return; }
                if (totalReferences >= CrmHistoryLimits.MaximumReferences) throw CrmHistorySourceFiles.Invalid();
                knownReferences.Add(reference); references.Add(reference); totalReferences++;
            }
            foreach (var reference in row.Metadata.References.Take(CrmHistoryLimits.MaximumRowReferences + 1)) AddReference(reference);
            if (row.Metadata.References.Count > CrmHistoryLimits.MaximumRowReferences) issues.Add("graph_limit_exceeded");
            if (row.Metadata.Entity == "quotes")
            {
                var children = versions[row.Metadata.SourceId].Take(CrmHistoryLimits.MaximumRowReferences + 1).ToArray();
                if (children.Length == 0) issues.Add("missing_quote_versions");
                if (children.Length > CrmHistoryLimits.MaximumRowReferences) issues.Add("graph_limit_exceeded");
                var current = CrmHistoryNativeSnapshotAdapter.OptionalNumber(row.Source, "current_version_number");
                if (current is null or <= 0) issues.Add("unsupported_payload_shape");
                if (ambiguousQuotes.Contains(row.Metadata.SourceId)) issues.Add("ambiguous_quote_version");
                CrmHistorySourceRow? currentVersion = current is > 0 && versionGroups.TryGetValue((row.Metadata.SourceId, current.Value), out var currentGroup) && currentGroup.Count == 1 ? currentGroup.First : null;
                if (currentVersion is null) issues.Add("missing_current_version");
                foreach (var child in children) AddReference(new CrmHistoryReference("quote_versions", child.Metadata.SourceId));
                if (currentVersion is null || !itemCounts.ContainsKey(currentVersion.Metadata.SourceId)) issues.Add("missing_quote_items");
            }
            else if (row.Metadata.Entity == "quote_versions")
            {
                var number = CrmHistoryNativeSnapshotAdapter.OptionalNumber(row.Source, "version_number");
                if (number is null or <= 0) issues.Add("unsupported_payload_shape");
                var quoteId = CrmHistoryNativeSnapshotAdapter.Text(row.Source, "quote_id") ?? "";
                if (number is > 0 && versionGroups.TryGetValue((quoteId, number.Value), out var group) && group.Count > 1) issues.Add("ambiguous_quote_version");
                var children = items[row.Metadata.SourceId].Take(CrmHistoryLimits.MaximumRowReferences + 1).ToArray();
                if (children.Length == 0) issues.Add("missing_quote_items");
                if (children.Length > CrmHistoryLimits.MaximumRowReferences) issues.Add("graph_limit_exceeded");
                foreach (var child in children) AddReference(new CrmHistoryReference("quote_items", child.Metadata.SourceId));
                var raw = CrmHistoryNativeSnapshotAdapter.Text(row.Source, "snapshot_json");
                if (raw is not null)
                {
                    try
                    {
                        using var snapshot = JsonDocument.Parse(raw);
                        if (snapshot.RootElement.ValueKind == JsonValueKind.Object && snapshot.RootElement.TryGetProperty("items", out var declared) && declared.ValueKind == JsonValueKind.Array
                            && declared.GetArrayLength() != itemCounts.GetValueOrDefault(row.Metadata.SourceId)) issues.Add("missing_quote_items");
                    }
                    catch (JsonException) { }
                }
            }
            result.Add(row.Metadata with { References = references.ToArray(), Issues = issues.Order(StringComparer.Ordinal).ToArray() });
        }
        return result;
    }
}
