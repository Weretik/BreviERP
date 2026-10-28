namespace Crm.Api.Quotes.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record CopyQuoteRequest(Guid RequestId, long ExpectedRevision, Guid VersionId);
