namespace Catalog.Api.Contracts.Products;

public sealed record ProductWriteRequest(
    string Type,
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
