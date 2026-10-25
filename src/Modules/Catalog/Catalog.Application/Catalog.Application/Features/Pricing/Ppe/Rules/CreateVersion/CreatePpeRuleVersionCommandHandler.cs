using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Validation;
namespace Catalog.Application.Features.Pricing.Ppe.Rules.CreateVersion;
public sealed class CreatePpeRuleVersionCommandHandler(IPpeRuleVersionWriter writer) : ICommandHandler<CreatePpeRuleVersionCommand, Result<PpeRuleVersion>>
{
 public async ValueTask<Result<PpeRuleVersion>> Handle(CreatePpeRuleVersionCommand command, CancellationToken cancellationToken)
 {
 if (!PpeVersionValidation.Request(command.RequestId, command.ExpectedVersionId, command.Actor) || !PpeVersionValidation.Rule(command.Values)) return Result.Invalid(new ValidationError("request", "Valid version, source, date, amounts and actor are required."));
 return await writer.AppendAsync(command.ExpectedVersionId, command.RequestId, command.Values, command.Actor, cancellationToken);
 }
}
