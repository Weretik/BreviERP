namespace Crm.Api.Quotes.Requests;

[System.Text.Json.Serialization.JsonUnmappedMemberHandling(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Disallow)]
public sealed record ArchiveQuoteRequest(Guid RequestId, long ExpectedRevision,
    [property: System.Text.Json.Serialization.JsonRequired] bool Archived, string Reason);
