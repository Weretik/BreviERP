using Catalog.Domain.Products.Enums;
using Catalog.Application.Contracts.Admin.Ppe;
using Catalog.Application.Contracts.Admin.Sewing;

namespace Catalog.Application.Contracts.Admin.Product;

public sealed record ProductAdminDetail(
    int Id,
    string Name,
    string RuName,
    string Slug,
    ProductType Type,
    string DescriptionUk,
    string DescriptionRu,
    IReadOnlyList<CategoryAdminDetail> Categories,
    IReadOnlyList<ProductPhotoDetail> Photos,
    IReadOnlyList<InformationBlockAdminDetail> InformationBlocks,
    IReadOnlyList<CharacteristicTableAdminDetail> CharacteristicTables,
    SewingAdminDetail? Sewing,
    PpeAdminDetail? Ppe,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
