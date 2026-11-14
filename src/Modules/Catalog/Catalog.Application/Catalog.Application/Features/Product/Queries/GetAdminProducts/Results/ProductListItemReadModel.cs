using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.Queries.GetAdminProducts.Results;

public sealed record ProductListItemReadModel(
    int Id,
    string Name,
    string Slug,
    ProductType Type,
    IReadOnlyList<int> CategoryIds,
    int? MainPhotoMediaFileId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
