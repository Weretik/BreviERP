using Catalog.Domain.Products.Enums;
using Catalog.Application.Features.Product.GetAdminList.DTOs;

namespace Catalog.Application.Features.Product.GetAdminList;

public sealed record GetAdminProductsQuery(int Page = 1, int PageSize = 20, string? Search = null, ProductType? Type = null, int? CategoryId = null, string SortBy = "name", bool Descending = false) : IQuery<Result<ProductListPage>>;
