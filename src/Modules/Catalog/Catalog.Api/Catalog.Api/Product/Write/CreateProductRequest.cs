using System.Text.Json.Serialization;
using Catalog.Api.Product.Write.Characteristics;
using Catalog.Api.Product.Write.Ppe;
using Catalog.Api.Product.Write.Sewing;

namespace Catalog.Api.Product.Write;

public sealed record CreateProductRequest(
    int Id,
    [property: JsonConverter(typeof(ProductWriteEnumStringJsonConverter))] string Type,
    string Name,
    string RuName,
    string DescriptionUk,
    string DescriptionRu,
    IReadOnlyCollection<int>? CategoryIds,
    IReadOnlyCollection<ProductPhotoWriteRequest>? Photos,
    IReadOnlyCollection<InformationBlockWriteRequest>? InformationBlocks,
    IReadOnlyCollection<CharacteristicTableWriteRequest>? CharacteristicTables,
    decimal? MetersPerProduct,
    IReadOnlyCollection<FabricWriteRequest>? Fabrics,
    IReadOnlyCollection<AccessoryWriteRequest>? Accessories,
    IReadOnlyCollection<int>? OperationIds,
    int? SupplierId,
    decimal? BasePrice,
    PercentWriteRequest? RetailPercent,
    PercentWriteRequest? WholesalePercent);
