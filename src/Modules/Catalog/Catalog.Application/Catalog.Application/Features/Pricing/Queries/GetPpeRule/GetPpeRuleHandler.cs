using Catalog.Application.Abstractions.Pricing;
namespace Catalog.Application.Features.Pricing.Queries.GetPpeRule;
public sealed class GetPpeRuleHandler(IPpePricingInputReader reader) : IQueryHandler<GetPpeRuleQuery, Result<PpeRuleVersion>>
{
    public async ValueTask<Result<PpeRuleVersion>> Handle(GetPpeRuleQuery query, CancellationToken cancellationToken)
    {
        var value = await reader.ReadRuleAsync(query.Code, cancellationToken);
        return value is null ? Result.NotFound() : Result.Success(value);
    }
}
