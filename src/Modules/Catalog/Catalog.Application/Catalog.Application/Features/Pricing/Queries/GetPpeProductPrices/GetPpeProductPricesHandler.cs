using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Features.Pricing.Shared;
using Catalog.Application.Features.Pricing.Shared.Ppe.Evaluation;
using Catalog.Application.Abstractions.Pricing.Ppe;
namespace Catalog.Application.Features.Pricing.Queries.GetPpeProductPrices;
public sealed class GetPpeProductPricesHandler(IPpePricingInputReader reader)
    : IQueryHandler<GetPpeProductPricesQuery, Result<IReadOnlyList<PpeProductPricing>>>
{
    public async ValueTask<Result<IReadOnlyList<PpeProductPricing>>> Handle(GetPpeProductPricesQuery query, CancellationToken cancellationToken)
    {
        var inputs = await reader.ReadAsync(query.ProductIds.ToArray(), cancellationToken);
        return Result.Success<IReadOnlyList<PpeProductPricing>>(inputs.Select(PpeProductPricingEvaluator.Evaluate).ToArray());
    }
}
