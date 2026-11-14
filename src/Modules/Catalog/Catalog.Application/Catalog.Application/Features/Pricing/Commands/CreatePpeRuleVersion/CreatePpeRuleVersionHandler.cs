using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Features.Pricing.Commands.CreatePpeRuleVersion;
public sealed class CreatePpeRuleVersionHandler(IPpeRuleVersionWriter writer) : ICommandHandler<CreatePpeRuleVersionCommand, Result<PpeRuleVersion>>
{
 public async ValueTask<Result<PpeRuleVersion>> Handle(CreatePpeRuleVersionCommand command, CancellationToken cancellationToken)
 {
 return await writer.AppendAsync(command.ExpectedVersionId, command.RequestId, command.Values, command.Actor, cancellationToken);
 }
}
