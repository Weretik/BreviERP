using Catalog.Application.Abstractions.Pricing;
namespace Catalog.Application.Features.Pricing.Queries.GetPpeRule;
public sealed record GetPpeRuleQuery(string Code) : IQuery<Result<PpeRuleVersion>>;
