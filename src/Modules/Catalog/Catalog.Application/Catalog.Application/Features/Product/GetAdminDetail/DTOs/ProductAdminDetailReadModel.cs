using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Features.Product.GetAdminDetail.DTOs;

public sealed record ProductAdminDetailReadModel(
    int Id,
    string Name,
    string RuName,
    string Slug,
    ProductType Type,
    string DescriptionUk,
    string DescriptionRu,
    IReadOnlyList<int> CategoryIds,
    IReadOnlyList<ProductPhotoReadModel> Photos,
    IReadOnlyList<InformationBlockReadModel> InformationBlocks,
    IReadOnlyList<CharacteristicTableReadModel> CharacteristicTables,
    SewingReadModel? Sewing,
    PpeReadModel? Ppe,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
