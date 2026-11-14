using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;
public sealed class CreatePpeOfferVersionHandler(IPpeOfferVersionWriter writer) : ICommandHandler<CreatePpeOfferVersionCommand, Result<PpeOfferVersion>>
{
 public async ValueTask<Result<PpeOfferVersion>> Handle(CreatePpeOfferVersionCommand command, CancellationToken cancellationToken)
 {
 return await writer.AppendAsync(command.ProductId, command.ExpectedVersionId, command.RequestId, command.Values, command.Actor, cancellationToken);
 }
}
