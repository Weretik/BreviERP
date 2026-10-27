using Crm.Application.Quotes.Pricing;

namespace Crm.Api.Quotes.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record PreviewQuotePricesRequest(
    Guid RequestId, Guid? QuoteId, QuotePriceIntent Intent);
