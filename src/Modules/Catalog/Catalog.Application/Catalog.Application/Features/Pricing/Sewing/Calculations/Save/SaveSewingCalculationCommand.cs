using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Calculations.Save;

public sealed record SaveSewingCalculationCommand(int ProductId, long ArticleId, Guid RequestId, string Actor)
    : ICommand<Result<SavedSewingCalculation>>;
