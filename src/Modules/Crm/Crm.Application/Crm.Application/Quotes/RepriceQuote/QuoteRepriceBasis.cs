namespace Crm.Application.Quotes.RepriceQuote;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record QuoteRepriceBasis(Guid VariantId, Guid BasisToken);
