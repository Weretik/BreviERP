namespace Crm.Application.Quotes.Pricing;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record QuotePriceOverride(string Tier, decimal Amount, string Reason);
