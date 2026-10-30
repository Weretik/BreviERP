using System.Text.Json.Serialization;

namespace Crm.Api.Quotes.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record PrepareQuoteRequest(Guid VersionId, long ExpectedRevision, Guid RequestId);
