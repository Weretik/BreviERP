namespace Catalog.Application.Features.Pricing.Queries.GetPpePriceSources;

public sealed record GetPpePriceSourcesQuery : IQuery<Result<IReadOnlyList<PpePriceSourceView>>>;
