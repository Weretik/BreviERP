using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Ppe.Offers.CreateVersion;
public sealed record CreatePpeOfferVersionCommand(int ProductId, Guid? ExpectedVersionId, Guid RequestId, PpeOfferValues Values, string Actor) : ICommand<Result<PpeOfferVersion>>;
