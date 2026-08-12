using Catalog.Domain.Products.Enums;

namespace Catalog.Application.Contracts.Admin;

public sealed record ProductAdminDetail(
    int Id,
    string Name,
    string RuName,
    string Slug,
    ProductType Type,
    string DescriptionUk,
    string DescriptionRu,
    IReadOnlyList<int> CategoryIds,
    IReadOnlyList<ProductPhotoDetail> Photos,
    IReadOnlyList<InformationBlockAdminDetail> InformationBlocks,
    IReadOnlyList<CharacteristicTableAdminDetail> CharacteristicTables,
    SewingAdminDetail? Sewing,
    PpeAdminDetail? Ppe,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
