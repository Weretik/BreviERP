namespace CatalogImport.PpePricing.Verification;

public sealed record PpePricingVerificationReport(string SpreadsheetId, string SourceHash, string SourceCapturedAt,
    DateTimeOffset CheckedAt, IReadOnlyList<PpePricingDiagnostic> Products)
{
    public int MatchedProducts => Products.Count(x => x.Comparisons.All(v => v.Status == "Matched"));
    public int IncompleteProducts => Products.Count(x => x.Comparisons.Any(v => v.Status != "Matched"));
    public int UnexplainedValues => Products.Sum(x => x.Comparisons.Count(v => v.Status is "Different" or "InvalidSourceValue" or "MissingCalculation"));
    public int DifferentValues => Products.Sum(x => x.Comparisons.Count(v => v.Status == "Different"));
}
