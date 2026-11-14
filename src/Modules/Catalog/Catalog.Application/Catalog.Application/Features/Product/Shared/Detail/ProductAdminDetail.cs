using Catalog.Domain.Products;
using Catalog.Domain.Products.Enums;
using Catalog.Application.Features.Product.Shared.Detail.Content;
using Catalog.Application.Features.Product.Shared.Detail.Ppe;
using Catalog.Application.Features.Product.Shared.Detail.Sewing;

namespace Catalog.Application.Features.Product.Shared.Detail;

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
