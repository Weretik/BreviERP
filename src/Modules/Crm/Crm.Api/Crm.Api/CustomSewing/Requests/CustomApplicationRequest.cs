using System.Text.Json.Serialization;

namespace Crm.Api.CustomSewing.Requests;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record CustomApplicationRequest(string Kind, string Location, Guid? ArtworkFileId,
    decimal Cost, decimal SaleAmount, bool SaleApproved);
