using Crm.Application.Imports.History.Contracts;
using Crm.Application.Imports.History.Validation;
namespace Crm.Application.Imports.History.Planning;

internal static class CrmHistoryRowClassifier
{
    private static readonly HashSet<string> InvalidIssues = new(StringComparer.Ordinal)
    {
        "invalid_snapshot_json", "invalid_changes_json", "invalid_before_json", "invalid_after_json", "invalid_payload_json",
        "truncated_json", "invalid_roles_json", "invalid_source_id", "invalid_row_shape", "invalid_file_metadata"
    };
    private static readonly HashSet<string> ReviewIssues = new(StringComparer.Ordinal)
    {
        "missing_current_version", "missing_quote_versions", "missing_quote_items", "unresolved_recipient",
        "missing_file_bytes", "missing_file_hash", "legacy_external_identity_unmapped", "legacy_target_reference_requires_mapping"
    };
    private static readonly HashSet<string> CounterpartyRoles = new(StringComparer.OrdinalIgnoreCase) { "CLIENT", "SUPPLIER", "FINANCIAL" };
    private static readonly HashSet<string> ItemKinds = new(StringComparer.Ordinal) { "sewing", "ppe", "one_off_ppe", "custom_sewing" };
    private static readonly HashSet<string> OwnedRoots = new(StringComparer.Ordinal) { "counterparties", "quotes", "custom_sewing_drafts", "introduction_saved_templates" };

    internal static void Classify(HistoryPlanningRow state,
        IReadOnlyDictionary<string, int> owners, IReadOnlyDictionary<string, Guid> identities,
        bool sourceConflict, bool stagedBefore, bool duplicate)
    {
        var row = state.Source;
        var invalid = row.Issues.Where(InvalidIssues.Contains).ToArray();
        if (!CrmHistoryInputRules.Identifier(row.SourceId) || invalid.Length != 0)
        {
            state.Status = HistoryDecisionStatus.Invalid;
            foreach (var reason in invalid) state.Reasons.Add(reason);
            if (!CrmHistoryInputRules.Identifier(row.SourceId)) state.Reasons.Add("invalid_source_id");
            return;
        }
        if (sourceConflict) { state.Review("source_key_payload_conflict"); return; }
        if (row.Entity == "counterparties" && row.Roles.Any(x => !CounterpartyRoles.Contains(x)))
        { state.Review("role_unresolved"); return; }
        if (row.Entity == "counterparties" && !row.Roles.Contains("CLIENT", StringComparer.OrdinalIgnoreCase))
        {
            if (row.Roles.Count != 0 && row.Roles.All(x => x.Equals("SUPPLIER", StringComparison.OrdinalIgnoreCase) || x.Equals("FINANCIAL", StringComparison.OrdinalIgnoreCase)))
            { state.Status = HistoryDecisionStatus.Excluded; state.Reasons.Add("non_client_role"); }
            else state.Review("role_unresolved");
            return;
        }
        foreach (var issue in row.Issues) state.Review(ReviewIssues.Contains(issue) ? issue : "row_source_issue");
        if (row.Entity == "quote_items" && (row.Kind is null || !ItemKinds.Contains(row.Kind))) state.Review("legacy_kind_unresolved");
        if (OwnedRoots.Contains(row.Entity) && row.OwnerKeys.Count == 0) state.Review("owner_missing");
        if (row.OwnerKeys.Any(x => !owners.ContainsKey(x))) state.Review("owner_unmapped");
        var targets = row.ExternalIdentityKeys.Where(identities.ContainsKey).Select(x => identities[x]).Distinct().Take(2).ToArray();
        if (targets.Length > 1) state.Review("exact_identity_conflict");
        if (state.Status == HistoryDecisionStatus.NeedsReview) return;
        if (targets.Length == 1) { state.Status = HistoryDecisionStatus.AlreadyMapped; state.Reasons.Add("exact_identity_mapping_supplied"); }
        else if (stagedBefore || duplicate)
        { state.Status = HistoryDecisionStatus.AlreadyStaged; state.Reasons.Add(stagedBefore ? "prior_staging_same_hash" : "duplicate_source_row"); }
    }
}
