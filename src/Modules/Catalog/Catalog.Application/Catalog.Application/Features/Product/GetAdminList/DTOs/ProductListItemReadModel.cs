using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.GetAdminList.DTOs;

public sealed record ProductListItemReadModel(
    int Id,
    string Name,
    string Slug,
    ProductType Type,
    IReadOnlyList<int> CategoryIds,
    int? MainPhotoMediaFileId,
    SewingReadModel? Sewing,
    PpeReadModel? Ppe,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
