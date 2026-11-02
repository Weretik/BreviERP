using Crm.Application.Imports.History.Contracts;
using Crm.Application.Imports.History.Validation;
namespace Crm.Application.Imports.History.Planning;

internal static class CrmHistoryReportBuilder
{
    internal static string[] Blockers(CrmHistorySnapshot source, CrmHistoryDryRunOptions options, CrmHistoryDryRunCounts counts, bool conflict)
    {
        var blockers = new HashSet<string>(StringComparer.Ordinal);
        if (!source.FullBackup) blockers.Add("source_not_full_backup");
        if (!source.TransactionConsistent) blockers.Add("source_not_transaction_consistent");
        if (source.Issues.Count != 0) blockers.Add("source_issues_present");
        if (source.Tables.Any(x => !x.Complete)) blockers.Add("source_table_incomplete");
        if (source.Tables.Any(x => x.Issues.Count != 0)) blockers.Add("source_table_issues_present");
        if (source.Tables.Any(x => x.ExpectedRows != x.ActualRows)) blockers.Add("source_table_count_mismatch");
        var byEntity = source.Rows.GroupBy(x => x.Entity).ToDictionary(x => x.Key, x => x.Count(), StringComparer.Ordinal);
        if (source.Tables.Any(x => x.ActualRows != byEntity.GetValueOrDefault(x.Entity)) || byEntity.Keys.Except(source.Tables.Select(x => x.Entity)).Any())
            blockers.Add("staged_row_count_mismatch");
        if (string.IsNullOrWhiteSpace(options.ScopeApprovalReference)) blockers.Add("scope_approval_missing");
        if (string.IsNullOrWhiteSpace(options.RetentionPolicyReference)) blockers.Add("retention_policy_missing");
        if (counts.NeedsReview != 0) blockers.Add("rows_need_review");
        if (counts.Invalid != 0) blockers.Add("invalid_source_rows");
        if (conflict) blockers.Add("staging_payload_conflicts");
        return blockers.Order(StringComparer.Ordinal).ToArray();
    }

    internal static CrmHistoryLedgerEntry[] Ledger(CrmHistoryDryRunOptions options,
        IReadOnlyDictionary<(string Entity, string Id), List<HistoryPlanningRow>> groups,
        IReadOnlySet<(string Entity, string Id)> conflicts, CancellationToken ct)
    {
        var entries = new Dictionary<(string Entity, string Id), CrmHistoryLedgerEntry>();
        foreach (var prior in options.PreviousStaging)
            entries[(prior.Entity, prior.SourceId)] = prior;
        foreach (var (key, members) in groups)
        {
            ct.ThrowIfCancellationRequested();
            if (entries.ContainsKey(key) || conflicts.Contains(key) || members.Any(x => x.Status == HistoryDecisionStatus.Invalid) || !CrmHistoryInputRules.Identifier(key.Id)) continue;
            entries[key] = new(key.Entity, key.Id, members[0].Source.PayloadSha256);
        }
        return entries.Values.OrderBy(x => x.Entity, StringComparer.Ordinal).ThenBy(x => x.SourceId, StringComparer.Ordinal).ToArray();
    }
}
