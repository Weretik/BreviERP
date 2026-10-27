namespace Crm.Application.Quotes.Pricing;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record QuotePriceIntent(
    string Kind,
    int? ProductId,
    long? ArticleId,
    QuotePriceOverride? Override,
    QuoteManualPrices? ManualPrices);
