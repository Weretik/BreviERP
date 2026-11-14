using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewItem;

/// <summary>One «Товари» row with its prices and fabric variants, for the product card header (CAT-013).</summary>
public sealed record GetProductOverviewItemQuery(int Id) : IQuery<Result<ProductOverviewItem>>;
