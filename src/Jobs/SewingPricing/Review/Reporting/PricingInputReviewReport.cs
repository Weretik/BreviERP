namespace SewingPricing.Jobs.Review;

public sealed record PricingInputReviewReport(string SpreadsheetId, string CatalogHash, string SettingsSourceHash,
    string PpeSourceHash, DateTimeOffset CheckedAt, IReadOnlyList<PricingReviewSubject> Subjects)
{
    public bool CanApply => false;
    public int MatchedSubjects => Subjects.Count(x => x.MatchesSource);
    public int DifferentSubjects => Subjects.Count - MatchedSubjects;
    public int HeldSubjects => Subjects.Count(x => x.HoldReasons.Count > 0);
}
