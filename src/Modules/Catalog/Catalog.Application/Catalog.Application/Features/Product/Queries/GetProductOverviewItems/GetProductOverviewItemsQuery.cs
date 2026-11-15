using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewItems;

/// <summary>«Товари» rows with prices and fabric variants for several products at once (the lines of a КП).</summary>
public sealed record GetProductOverviewItemsQuery(IReadOnlyCollection<int> Ids)
    : IQuery<Result<IReadOnlyList<ProductOverviewItem>>>;
