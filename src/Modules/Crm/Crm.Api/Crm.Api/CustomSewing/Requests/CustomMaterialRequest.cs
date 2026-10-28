using System.Text.Json.Serialization;

namespace Crm.Api.CustomSewing.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CustomMaterialRequest(int ReferenceId, decimal Quantity, string Unit);
