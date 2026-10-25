namespace SewingPricing.Jobs.Verification;

public sealed record PricingParityDifference(string Component, decimal Expected, decimal? Actual);

