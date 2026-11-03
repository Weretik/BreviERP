using Catalog.Application.Features.Product.Overview.Prices.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Overview.Prices;

/// <summary>Prices of every product of one «Товари» tab for the list (CAT-011).</summary>
public sealed record GetProductOverviewPricesQuery(ProductType Type) : IQuery<Result<IReadOnlyList<ProductOverviewPrices>>>;
