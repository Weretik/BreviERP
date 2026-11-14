using Catalog.Application.Abstractions.Pricing;

namespace Catalog.Application.Features.Pricing.Commands.PublishSewingCalculation;

public sealed record PublishSewingCalculationCommand(int ProductId, long ArticleId, Guid CalculationId, string Actor)
    : ICommand<Result<PublishedSewingPrice>>;
