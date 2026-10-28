namespace Crm.Api.Quotes.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record TransferQuoteRequest(Guid RequestId, long ExpectedRevision, int OwnerId, string Reason);
