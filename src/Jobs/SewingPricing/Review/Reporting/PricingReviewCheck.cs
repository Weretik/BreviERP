namespace SewingPricing.Jobs.Review;

public sealed record PricingReviewCheck(string Field, string? Expected, string? Actual)
{
    public bool Matches => Expected == Actual;
}
