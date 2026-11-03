using Catalog.Application.Features.Product.Overview.GetList.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Overview.GetList;

/// <summary>All products of one type (or all), sorted by code then name; an empty list when there are none.</summary>
public sealed record GetProductOverviewQuery(ProductType? Type) : IQuery<Result<IReadOnlyList<ProductOverviewRow>>>;
