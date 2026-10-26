namespace SewingPricing.Jobs.Review;

public sealed record PricingReviewSubject(string Key, string Group, string? Version, IReadOnlyList<PricingReviewCheck> Checks)
{
    public IReadOnlyList<string> HoldReasons { get; init; } = [];
    public bool? IsApproved { get; init; }
    public bool? IsReviewed { get; init; }
    public bool MatchesSource => Checks.Count > 0 && Checks.All(x => x.Matches);
    public bool CanConfirm => false;
}
