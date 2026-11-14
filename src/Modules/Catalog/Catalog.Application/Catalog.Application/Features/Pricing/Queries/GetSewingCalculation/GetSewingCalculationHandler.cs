using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Calculations;

namespace Catalog.Application.Features.Pricing.Queries.GetSewingCalculation;

public sealed class GetSewingCalculationHandler(ISewingCalculationStore store)
    : IQueryHandler<GetSewingCalculationQuery, Result<SavedSewingCalculation>>
{
    public async ValueTask<Result<SavedSewingCalculation>> Handle(
        GetSewingCalculationQuery query, CancellationToken cancellationToken)
    {
        var saved = await store.FindAsync(query.ProductId, query.ArticleId, query.CalculationId, cancellationToken);
        return saved is null ? Result.NotFound() : Result.Success(saved);
    }
}
