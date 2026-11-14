using Catalog.Application.Contracts.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Ppe.Rules.GetCurrent;
public sealed class GetPpeRuleQueryHandler(IPpePricingInputReader reader) : IQueryHandler<GetPpeRuleQuery, Result<PpeRuleVersion>>
{
    public async ValueTask<Result<PpeRuleVersion>> Handle(GetPpeRuleQuery query, CancellationToken cancellationToken)
    {
        var value = await reader.ReadRuleAsync(query.Code, cancellationToken);
        return value is null ? Result.NotFound() : Result.Success(value);
    }
}
