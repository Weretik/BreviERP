using System.ComponentModel.DataAnnotations;
using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Api.Contracts.Pricing.Ppe;
public sealed record CreatePpeRuleVersionRequest(Guid? ExpectedVersionId, Guid RequestId, [Required] PpeRuleValues Values);
