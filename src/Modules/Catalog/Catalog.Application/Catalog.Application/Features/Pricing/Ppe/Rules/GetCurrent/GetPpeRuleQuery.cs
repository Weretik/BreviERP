using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Ppe.Rules.GetCurrent;
public sealed record GetPpeRuleQuery(string Code) : IQuery<Result<PpeRuleVersion>>;
