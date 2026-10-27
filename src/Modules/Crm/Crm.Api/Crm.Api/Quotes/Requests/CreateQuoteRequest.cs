using Crm.Application.Quotes.Contracts.Inputs;

namespace Crm.Api.Quotes.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]

public sealed record CreateQuoteRequest(
    Guid RequestId, Guid? CustomerPublicId, Guid? ContactPublicId,
    string? RecipientName, QuoteDraftContent Content);
