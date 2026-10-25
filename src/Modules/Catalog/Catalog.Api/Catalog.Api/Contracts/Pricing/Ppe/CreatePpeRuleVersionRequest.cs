using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Catalog.Api.Contracts.Pricing.Ppe;
public sealed record CreatePpeRuleVersionRequest(
    [property: JsonRequired] Guid? ExpectedVersionId,
    [property: JsonRequired] Guid RequestId,
    [property: JsonRequired] [Required] PpeRuleValuesRequest Values);
