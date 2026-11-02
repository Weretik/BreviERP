namespace Crm.Application.Imports.History.Planning;

internal static class CrmHistoryDependencyPlanner
{
    internal static void Propagate(IReadOnlyList<HistoryPlanningRow> rows,
        IReadOnlyDictionary<(string Entity, string Id), List<HistoryPlanningRow>> groups, CancellationToken ct)
    {
        var consumers = new Dictionary<(string Entity, string Id), List<HistoryPlanningRow>>();
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            foreach (var reference in row.Source.References)
            {
                var key = (reference.Entity, reference.SourceId);
                if (!groups.ContainsKey(key)) { row.Review("missing_dependency"); continue; }
                if (!consumers.TryGetValue(key, out var list)) consumers[key] = list = [];
                list.Add(row);
            }
        }
        var bad = new HashSet<(string Entity, string Id)>();
        var queue = new Queue<(string Entity, string Id)>();
        void AddUnresolved((string Entity, string Id) key)
        {
            if (!bad.Add(key)) return;
            foreach (var member in groups[key]) member.Review("source_group_unresolved");
            queue.Enqueue(key);
        }
        foreach (var (key, members) in groups)
        {
            ct.ThrowIfCancellationRequested();
            if (members.Any(x => x.IsUnresolved)) AddUnresolved(key);
        }
        while (queue.TryDequeue(out var key))
        {
            ct.ThrowIfCancellationRequested();
            if (!consumers.TryGetValue(key, out var children)) continue;
            var target = groups[key];
            var reason = target.Any(x => x.Status == HistoryDecisionStatus.Invalid) ? "dependency_invalid" :
                target.All(x => x.Status == HistoryDecisionStatus.Excluded) ? "dependency_excluded" : "dependency_needs_review";
            foreach (var child in children)
            {
                child.Review(reason);
                if (child.IsUnresolved) AddUnresolved(child.Key);
            }
        }
    }
}
