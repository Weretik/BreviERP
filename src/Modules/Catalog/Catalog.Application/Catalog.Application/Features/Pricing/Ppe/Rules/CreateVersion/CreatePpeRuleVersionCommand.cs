using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Ppe.Rules.CreateVersion;
public sealed record CreatePpeRuleVersionCommand(Guid? ExpectedVersionId, Guid RequestId, PpeRuleValues Values, string Actor) : ICommand<Result<PpeRuleVersion>>;
