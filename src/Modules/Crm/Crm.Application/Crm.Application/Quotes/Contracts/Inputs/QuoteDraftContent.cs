namespace Crm.Application.Quotes.Contracts.Inputs;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record QuoteDraftContent(IReadOnlyList<QuoteItemInput> Items);
