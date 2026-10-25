namespace SewingPricing.Jobs.Verification;

public sealed record SewingPricingParityValue(string Component, string SourceCell, decimal Expected,
    decimal? Actual, decimal? Difference, bool Matches);
