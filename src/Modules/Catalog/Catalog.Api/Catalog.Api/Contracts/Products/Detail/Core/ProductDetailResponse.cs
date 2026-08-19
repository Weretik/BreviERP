namespace Catalog.Api.Contracts.Products;

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
