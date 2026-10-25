namespace CatalogImport.PpePricing.Verification;

public sealed record PpePricingParityValue(string Tier, string SourceCell, string SourceRaw,
    decimal? SourcePrice, decimal? CalculatedPrice, decimal? Difference, string Status);
