using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;
public sealed record CreatePpeOfferVersionCommand(int ProductId, Guid? ExpectedVersionId, Guid RequestId, PpeOfferValues Values, string Actor) : ICommand<Result<PpeOfferVersion>>;
