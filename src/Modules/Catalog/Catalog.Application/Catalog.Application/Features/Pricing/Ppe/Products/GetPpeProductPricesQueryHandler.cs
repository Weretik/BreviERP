using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
namespace Catalog.Application.Features.Pricing.Ppe.Products;
public sealed class GetPpeProductPricesQueryHandler(IPpePricingInputReader reader)
    : IQueryHandler<GetPpeProductPricesQuery, Result<IReadOnlyList<PpeProductPricing>>>
{
    public async ValueTask<Result<IReadOnlyList<PpeProductPricing>>> Handle(GetPpeProductPricesQuery query, CancellationToken cancellationToken)
    {
        var inputs = await reader.ReadAsync(query.ProductIds.ToArray(), cancellationToken);
        return Result.Success<IReadOnlyList<PpeProductPricing>>(inputs.Select(PpeProductPricingEvaluator.Evaluate).ToArray());
    }
}
