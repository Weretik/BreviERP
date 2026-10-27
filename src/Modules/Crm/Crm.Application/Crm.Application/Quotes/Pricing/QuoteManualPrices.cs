namespace Crm.Application.Quotes.Pricing;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record QuoteManualPrices(decimal Retail, decimal Wholesale, decimal LargeWholesale, string Reason);
