using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;
using Catalog.Application.Features.Product.Queries.GetAdminProducts.Results;

namespace Catalog.Application.Features.Product.Queries.GetAdminProducts;

/// <param name="SupplierId">The supplier card tab «Товари ЗІЗ» (CARD-001 R-08): only products of this supplier.</param>
public sealed record GetAdminProductsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    ProductType? Type = null,
    int? CategoryId = null,
    string SortBy = "name",
    bool Descending = false,
    int? SupplierId = null)
    : IQuery<PagedResult<IReadOnlyList<ProductListItem>>>;
