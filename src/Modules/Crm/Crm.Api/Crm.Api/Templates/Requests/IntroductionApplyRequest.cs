using System.Text.Json.Serialization;

namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record IntroductionApplyRequest(
    [property: JsonRequired] Guid RequestId,
    Guid? VersionId,
    Guid? CustomerPublicId,
    Guid? ContactPublicId,
    string? RecipientName);
