using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Queries.GetAdminProducts;

public sealed record ProductListItem(
    int Id,
    string Name,
    string Slug,
    ProductType Type,
    IReadOnlyList<int> CategoryIds,
    ProductMainPhoto? MainPhoto,
    decimal MinimumWholesalePrice,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
