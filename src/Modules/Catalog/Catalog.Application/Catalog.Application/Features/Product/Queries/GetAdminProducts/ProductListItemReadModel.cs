using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Queries.GetAdminProducts;

public sealed record ProductListItemReadModel(
    int Id,
    string Name,
    string Slug,
    ProductType Type,
    IReadOnlyList<int> CategoryIds,
    int? MainPhotoMediaFileId,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
