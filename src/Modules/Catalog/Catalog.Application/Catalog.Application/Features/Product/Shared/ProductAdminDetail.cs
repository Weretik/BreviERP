using Catalog.Domain.Products;

namespace Catalog.Application.Features.Product.Shared;

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
