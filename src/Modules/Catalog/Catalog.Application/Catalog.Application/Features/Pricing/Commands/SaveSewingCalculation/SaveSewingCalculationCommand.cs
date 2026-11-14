using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Calculations;

namespace Catalog.Application.Features.Pricing.Commands.SaveSewingCalculation;

public sealed record SaveSewingCalculationCommand(int ProductId, long ArticleId, Guid RequestId, string Actor)
    : ICommand<Result<SavedSewingCalculation>>;
