using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Ppe.Versions;
namespace Catalog.Application.Features.Pricing.Queries.GetPpeRule;
public sealed record GetPpeRuleQuery(string Code) : IQuery<Result<PpeRuleVersion>>;
