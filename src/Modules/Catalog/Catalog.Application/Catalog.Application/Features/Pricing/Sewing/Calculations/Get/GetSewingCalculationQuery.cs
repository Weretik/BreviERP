using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Calculations.Get;

public sealed record GetSewingCalculationQuery(int ProductId, long ArticleId, Guid CalculationId)
    : IQuery<Result<SavedSewingCalculation>>;
