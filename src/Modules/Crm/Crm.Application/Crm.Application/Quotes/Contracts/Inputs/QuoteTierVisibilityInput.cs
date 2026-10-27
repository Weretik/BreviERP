namespace Crm.Application.Quotes.Contracts.Inputs;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record QuoteTierVisibilityInput(bool Retail, bool Wholesale, bool LargeWholesale);
