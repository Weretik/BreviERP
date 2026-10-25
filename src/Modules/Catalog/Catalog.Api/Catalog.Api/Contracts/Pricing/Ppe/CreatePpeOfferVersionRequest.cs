using System.ComponentModel.DataAnnotations;
using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Api.Contracts.Pricing.Ppe;
public sealed record CreatePpeOfferVersionRequest(Guid? ExpectedVersionId, Guid RequestId, [Required] PpeOfferValues Values);
