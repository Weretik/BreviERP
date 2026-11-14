using Catalog.Api.Endpoints.Product.Shared;
using Catalog.Api.Endpoints.Product.Detail.Content;
using Catalog.Api.Endpoints.Product.Detail.Ppe;
using Catalog.Api.Endpoints.Product.Detail.Sewing;

namespace Catalog.Api.Endpoints.Product.Detail;

public sealed record ProductDetailResponse(
    int Id,
    string Name,
    string RuName,
    string Slug,
    string Type,
    IReadOnlyList<int> CategoryIds,
    ProductMainPhotoResponse? MainPhoto,
    decimal MinimumWholesalePrice,
    string DescriptionUk,
    string DescriptionRu,
    IReadOnlyList<CategoryResponse> Categories,
    IReadOnlyList<ProductPhotoResponse> Photos,
    IReadOnlyList<InformationBlockResponse> InformationBlocks,
    IReadOnlyList<CharacteristicTableResponse> CharacteristicTables,
    SewingResponse? Sewing,
    PpeResponse? Ppe,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
