using System.Text.Json.Serialization;

namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record IntroductionTemplateWriteRequest(
    [property: JsonRequired] Guid RequestId,
    [property: JsonRequired] string Name,
    [property: JsonRequired] string Layout,
    [property: JsonRequired] Guid EmailTemplateVersionId,
    [property: JsonRequired] IReadOnlyList<IntroductionSlotSelectionRequest> Selections,
    long? ExpectedRevision = null);
