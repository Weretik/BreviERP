using System.Text.Json.Serialization;

namespace Crm.Api.Templates.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record IntroductionSlotSelectionRequest(
    [property: JsonRequired] int SlotUid,
    [property: JsonRequired] int ProductId,
    [property: JsonRequired] long ArticleId);
