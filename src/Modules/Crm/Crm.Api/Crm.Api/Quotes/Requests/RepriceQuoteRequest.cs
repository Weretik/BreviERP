using Crm.Application.Quotes.RepriceQuote;

namespace Crm.Api.Quotes.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record RepriceQuoteRequest(Guid RequestId, long ExpectedRevision, Guid VersionId, IReadOnlyList<QuoteRepriceBasis> Bases);
