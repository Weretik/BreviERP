using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Validation;
namespace Catalog.Application.Features.Pricing.Ppe.Rules.CreateVersion;
public sealed class CreatePpeRuleVersionCommandHandler(IPpeRuleVersionWriter writer) : ICommandHandler<CreatePpeRuleVersionCommand, Result<PpeRuleVersion>>
{
 public async ValueTask<Result<PpeRuleVersion>> Handle(CreatePpeRuleVersionCommand command, CancellationToken cancellationToken)
 {
 return await writer.AppendAsync(command.ExpectedVersionId, command.RequestId, command.Values, command.Actor, cancellationToken);
 }
}
