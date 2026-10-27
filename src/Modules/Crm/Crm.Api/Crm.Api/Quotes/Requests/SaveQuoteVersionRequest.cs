using Crm.Application.Quotes.Contracts.Inputs;

namespace Crm.Api.Quotes.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record SaveQuoteVersionRequest(
    Guid RequestId, long ExpectedRevision, QuoteDraftContent Content);
