using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Validation;
namespace Catalog.Application.Features.Pricing.Ppe.Offers.CreateVersion;
public sealed class CreatePpeOfferVersionCommandHandler(IPpeOfferVersionWriter writer) : ICommandHandler<CreatePpeOfferVersionCommand, Result<PpeOfferVersion>>
{
 public async ValueTask<Result<PpeOfferVersion>> Handle(CreatePpeOfferVersionCommand command, CancellationToken cancellationToken)
 {
 return await writer.AppendAsync(command.ProductId, command.ExpectedVersionId, command.RequestId, command.Values, command.Actor, cancellationToken);
 }
}
