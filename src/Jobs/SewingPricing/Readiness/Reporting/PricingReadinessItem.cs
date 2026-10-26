namespace SewingPricing.Jobs.Readiness;

public sealed record PricingReadinessItem(
    string SaleSku, string Kind, int? ProductId, long? ArticleId, bool CalculationReady,
    IReadOnlyList<PricingReadinessBlocker> Blockers, IReadOnlyList<string> SourceIssues)
{
    public Guid? OfferVersionId { get; init; }
    public Guid? RuleVersionId { get; init; }
    public bool? OfferApproved { get; init; }
    public bool? RuleApproved { get; init; }
    public long? SettingsVersionId { get; init; }
    public IReadOnlyList<long> ReferenceVersionIds { get; init; } = [];
    public Guid? PublicationId { get; init; }
    public string NextAction => SourceIssues.Count > 0 ? "ReviewSourceData"
        : !CalculationReady ? "ResolveBlockers"
        : Kind == "Ppe" ? "UseCurrentPricing"
        : PublicationId.HasValue ? "ReviewCurrentPublication" : "SaveAndPublish";
}
