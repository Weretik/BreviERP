using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Validation;
namespace Catalog.Application.Features.Pricing.Ppe.Offers.CreateVersion;
public sealed class CreatePpeOfferVersionCommandHandler(IPpeOfferVersionWriter writer) : ICommandHandler<CreatePpeOfferVersionCommand, Result<PpeOfferVersion>>
{
 public async ValueTask<Result<PpeOfferVersion>> Handle(CreatePpeOfferVersionCommand command, CancellationToken cancellationToken)
 {
 if (!PpeVersionValidation.Request(command.RequestId, command.ExpectedVersionId, command.Actor) || !PpeVersionValidation.Offer(command.Values) || command.ProductId <= 0) return Result.Invalid(new ValidationError("request", "Valid version, source, date, amounts and actor are required."));
 return await writer.AppendAsync(command.ProductId, command.ExpectedVersionId, command.RequestId, command.Values, command.Actor, cancellationToken);
 }
}
