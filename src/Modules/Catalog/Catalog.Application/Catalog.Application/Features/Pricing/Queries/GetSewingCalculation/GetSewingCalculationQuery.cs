using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Calculations;

namespace Catalog.Application.Features.Pricing.Queries.GetSewingCalculation;

public sealed record GetSewingCalculationQuery(int ProductId, long ArticleId, Guid CalculationId)
    : IQuery<Result<SavedSewingCalculation>>;
