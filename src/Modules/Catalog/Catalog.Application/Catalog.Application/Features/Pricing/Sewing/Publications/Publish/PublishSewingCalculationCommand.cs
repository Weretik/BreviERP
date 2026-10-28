using Catalog.Application.Contracts.Pricing;

namespace Catalog.Application.Features.Pricing.Sewing.Publications.Publish;

public sealed record PublishSewingCalculationCommand(int ProductId, long ArticleId, Guid CalculationId, string Actor)
    : ICommand<Result<PublishedSewingPrice>>;
