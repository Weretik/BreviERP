using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Create.DTOs;

public sealed record CreateProductCommandRequest(
    int Id,
    string Name,
    string RuName,
    ProductType Type,
    string DescriptionUk,
    string DescriptionRu,
    IReadOnlyCollection<int> CategoryIds,
    IReadOnlyCollection<ProductPhotoRequest> Photos,
    IReadOnlyCollection<InformationBlockRequest> InformationBlocks,
    IReadOnlyCollection<CharacteristicTableRequest> CharacteristicTables,
    SewingProductRequest? Sewing,
    PpeProductRequest? Ppe);

public sealed record ProductPhotoRequest(int MediaFileId, string? Alt, bool IsVisible, bool IsMain, int SortOrder);
public sealed record InformationBlockRequest(string TitleUk, string TitleRu, string TextUk, string TextRu, int SortOrder);
public sealed record CharacteristicTableRequest(string TitleUk, string TitleRu, int SortOrder, IReadOnlyCollection<CharacteristicRowRequest> Rows);
public sealed record CharacteristicRowRequest(string LabelUk, string LabelRu, string ValueUk, string ValueRu, int SortOrder);
public sealed record SewingProductRequest(decimal MetersPerProduct, IReadOnlyCollection<FabricRequest> Fabrics,
    IReadOnlyCollection<AccessoryRequest> Accessories, IReadOnlyCollection<int> OperationIds);
public sealed record FabricRequest(int FabricId, bool IsPrimary, int SortOrder);
public sealed record AccessoryRequest(int GarmentAccessoryId, decimal Quantity, int SortOrder);
public sealed record PpeProductRequest(int SupplierId, decimal BasePrice, PercentRequest RetailPercent, PercentRequest WholesalePercent);
public sealed record PercentRequest(PricePercentSource Source, int? AdditionalReferenceId, decimal? CustomPercent);
