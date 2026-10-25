using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Calculations.Get;

public sealed class GetSewingCalculationQueryHandler(ISewingCalculationStore store)
    : IQueryHandler<GetSewingCalculationQuery, Result<SavedSewingCalculation>>
{
    public async ValueTask<Result<SavedSewingCalculation>> Handle(
        GetSewingCalculationQuery query, CancellationToken cancellationToken)
    {
        var saved = await store.FindAsync(query.ProductId, query.ArticleId, query.CalculationId, cancellationToken);
        return saved is null ? Result.NotFound() : Result.Success(saved);
    }
}
