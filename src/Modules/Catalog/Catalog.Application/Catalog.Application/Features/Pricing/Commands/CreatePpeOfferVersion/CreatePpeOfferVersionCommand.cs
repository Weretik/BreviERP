using Catalog.Application.Abstractions.Pricing;
namespace Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;
public sealed record CreatePpeOfferVersionCommand(int ProductId, Guid? ExpectedVersionId, Guid RequestId, PpeOfferValues Values, string Actor) : ICommand<Result<PpeOfferVersion>>;
