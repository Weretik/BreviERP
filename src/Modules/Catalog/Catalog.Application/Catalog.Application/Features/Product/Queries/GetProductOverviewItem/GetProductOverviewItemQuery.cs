using Catalog.Application.Features.Product.Queries.GetProductOverviewPage;
using Catalog.Application.Features.Product.Queries.GetProductOverviewPage.Results;

namespace Catalog.Application.Features.Product.Queries.GetProductOverviewItem;

/// <summary>One «Товари» row with its prices and fabric variants, for the product card header (CAT-013).</summary>
public sealed record GetProductOverviewItemQuery(int Id) : IQuery<Result<ProductOverviewItem>>;
