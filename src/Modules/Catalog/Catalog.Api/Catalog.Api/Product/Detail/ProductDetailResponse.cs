using Catalog.Api.Product.Detail.Categories;
using Catalog.Api.Product.Detail.Characteristics;
using Catalog.Api.Product.Detail.Content;
using Catalog.Api.Product.Detail.Media;
using Catalog.Api.Product.Detail.Ppe;
using Catalog.Api.Product.Detail.Sewing;
using Catalog.Api.Product.Shared;
namespace Catalog.Api.Product.Detail;

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
