using Catalog.Application.Contracts.Pricing.Ppe;
using Catalog.Application.Features.Pricing.Ppe.Evaluation;
namespace Catalog.Application.Features.Pricing.Ppe.Products;
public sealed class GetPpeProductPricesQueryHandler(IPpePricingInputReader reader)
    : IQueryHandler<GetPpeProductPricesQuery, Result<IReadOnlyList<PpeProductPricing>>>
{
    public async ValueTask<Result<IReadOnlyList<PpeProductPricing>>> Handle(GetPpeProductPricesQuery query, CancellationToken cancellationToken)
    {
        if (query.ProductIds is null || query.ProductIds.Count is < 1 or > 100 || query.ProductIds.Any(x=>x<=0)
            || query.ProductIds.Distinct().Count()!=query.ProductIds.Count)
            return Result.Invalid(new ValidationError("productIds", "Supply 1–100 distinct positive Product IDs."));
        var inputs = await reader.ReadAsync(query.ProductIds.ToArray(), cancellationToken);
        return Result.Success<IReadOnlyList<PpeProductPricing>>(inputs.Select(PpeProductPricingEvaluator.Evaluate).ToArray());
    }
}
