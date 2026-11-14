using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Features.Pricing.Commands.CreatePpeRuleVersion;
public sealed record CreatePpeRuleVersionCommand(Guid? ExpectedVersionId, Guid RequestId, PpeRuleValues Values, string Actor) : ICommand<Result<PpeRuleVersion>>;
