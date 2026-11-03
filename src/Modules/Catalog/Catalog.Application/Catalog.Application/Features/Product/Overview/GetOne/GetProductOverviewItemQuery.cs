using Catalog.Application.Features.Product.Overview.GetList.DTOs;

namespace Catalog.Application.Features.Product.Overview.GetOne;

/// <summary>One «Товари» row with its prices and fabric variants, for the product card header (CAT-013).</summary>
public sealed record GetProductOverviewItemQuery(int Id) : IQuery<Result<ProductOverviewItem>>;
