namespace SewingPricing.Jobs.Readiness;

public sealed record PricingReadinessReport(
    string SpreadsheetId, string SourceHash, string SourceCapturedAt,
    DateTimeOffset CheckedAt, IReadOnlyList<PricingReadinessItem> Items)
{
    public bool CanActivate => false;
    public int CalculationReadyCount => Items.Count(x => x.CalculationReady);
    public int SourceIssueCount => Items.Count(x => x.SourceIssues.Count != 0);
    public IReadOnlyDictionary<string, int> Actions => Items.GroupBy(x => x.NextAction).ToDictionary(x => x.Key, x => x.Count());
    public IReadOnlyDictionary<string, int> Blockers => Items.SelectMany(x => x.Blockers.Select(b => b.Code).Distinct())
        .GroupBy(x => x).ToDictionary(x => x.Key, x => x.Count());
}
