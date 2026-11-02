using Ardalis.Result;
using Crm.Application.Imports.History.Contracts;
using Crm.Application.Imports.History.Validation;
namespace Crm.Application.Imports.History.Planning;

public sealed class CrmHistoryDryRunPlanner
{
    public Result<CrmHistoryDryRunReport> Plan(CrmHistoryDryRunRequest request, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        if (request is null || !new CrmHistoryDryRunValidator().Validate(request).IsValid)
            return Result<CrmHistoryDryRunReport>.Invalid(new ValidationError("history", "history_dry_run_invalid_shape"));
        var source = request.Snapshot;
        var options = request.Options;
        var owners = options.OwnerMappings.ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
        var identities = options.ExactIdentityMappings.ToDictionary(x => x.Key, x => Guid.Parse(x.Value), StringComparer.OrdinalIgnoreCase);
        var previous = new Dictionary<(string Entity, string Id), string>();
        foreach (var entry in options.PreviousStaging)
        { ct.ThrowIfCancellationRequested(); previous[(entry.Entity, entry.SourceId)] = entry.PayloadSha256; }
        var rows = source.Rows.Select(x => new HistoryPlanningRow(x)).ToArray();
        var groups = new Dictionary<(string Entity, string Id), List<HistoryPlanningRow>>();
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            if (!groups.TryGetValue(row.Key, out var members)) groups[row.Key] = members = [];
            members.Add(row);
        }
        var conflicts = new HashSet<(string Entity, string Id)>();
        foreach (var (key, members) in groups)
        {
            ct.ThrowIfCancellationRequested();
            var hash = members[0].Source.PayloadSha256;
            if (members.Any(x => !hash.Equals(x.Source.PayloadSha256, StringComparison.OrdinalIgnoreCase)) ||
                previous.TryGetValue(key, out var oldHash) && !hash.Equals(oldHash, StringComparison.OrdinalIgnoreCase)) conflicts.Add(key);
            foreach (var row in members)
                CrmHistoryRowClassifier.Classify(row, owners, identities, conflicts.Contains(key), previous.ContainsKey(key), row != members[0]);
        }
        CrmHistoryDependencyPlanner.Propagate(rows, groups, ct);
        var counts = new CrmHistoryDryRunCounts(rows.Length,
            rows.Count(x => x.Status == HistoryDecisionStatus.WouldImport), rows.Count(x => x.Status == HistoryDecisionStatus.AlreadyStaged),
            rows.Count(x => x.Status == HistoryDecisionStatus.AlreadyMapped), rows.Count(x => x.Status == HistoryDecisionStatus.NeedsReview),
            rows.Count(x => x.Status == HistoryDecisionStatus.Excluded), rows.Count(x => x.Status == HistoryDecisionStatus.Invalid));
        var blockers = CrmHistoryReportBuilder.Blockers(source, options, counts, conflicts.Count != 0);
        var ledger = CrmHistoryReportBuilder.Ledger(options, groups, conflicts, ct);
        var report = new CrmHistoryDryRunReport("dry-run", source.DatasetId, source.DatabaseSha256, source.ManifestSha256,
            blockers.Length == 0 ? "ready_for_review" : "blocked", 0, false, counts, blockers,
            source.Tables.Select(x => x with { Issues = x.Issues.Count == 0 ? [] : new[] { "table_source_issue" } }).ToArray(),
            rows.Select(x => x.Decision()).ToArray(), ledger)
        { ControlSha256 = source.ControlSha256, OptionsSha256 = source.OptionsSha256 };
        return Result.Success(report);
    }
}
