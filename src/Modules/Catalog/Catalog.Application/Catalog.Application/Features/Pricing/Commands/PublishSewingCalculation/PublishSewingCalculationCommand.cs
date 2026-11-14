using Catalog.Application.Abstractions.Pricing;
using Catalog.Application.Abstractions.Pricing.Publications;

namespace Catalog.Application.Features.Pricing.Commands.PublishSewingCalculation;

public sealed record PublishSewingCalculationCommand(int ProductId, long ArticleId, Guid CalculationId, string Actor)
    : ICommand<Result<PublishedSewingPrice>>;
