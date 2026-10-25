using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Catalog.Api.Contracts.Pricing.Ppe;
public sealed record CreatePpeOfferVersionRequest(
    [property: JsonRequired] Guid? ExpectedVersionId,
    [property: JsonRequired] Guid RequestId,
    [property: JsonRequired] [Required] PpeOfferValuesRequest Values);
