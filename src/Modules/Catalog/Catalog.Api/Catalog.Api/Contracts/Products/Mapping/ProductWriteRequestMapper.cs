using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Api.Contracts.Products;

public static class ProductWriteRequestMapper
{
    public static bool TryMap(CreateProductRequest request, out CreateProductCommandRequest? result)
        => TryMap(request.Id, request.Type, request.Name, request.RuName, request.DescriptionUk, request.DescriptionRu, request.CategoryIds, request.Photos, request.InformationBlocks, request.CharacteristicTables, request.MetersPerProduct, request.Fabrics, request.Accessories, request.OperationIds, request.SupplierId, request.BasePrice, request.RetailPercent, request.WholesalePercent, out result);

    public static bool TryMap(ProductWriteRequest request, out CreateProductCommandRequest? result)
        => TryMap(0, request.Type, request.Name, request.RuName, request.DescriptionUk, request.DescriptionRu, request.CategoryIds, request.Photos, request.InformationBlocks, request.CharacteristicTables, request.MetersPerProduct, request.Fabrics, request.Accessories, request.OperationIds, request.SupplierId, request.BasePrice, request.RetailPercent, request.WholesalePercent, out result);

    private static bool TryMap(int id, string type, string name, string ruName, string descriptionUk, string descriptionRu, IReadOnlyCollection<int>? categoryIds, IReadOnlyCollection<ProductPhotoWriteRequest>? photos, IReadOnlyCollection<InformationBlockWriteRequest>? informationBlocks, IReadOnlyCollection<CharacteristicTableWriteRequest>? characteristicTables, decimal? metersPerProduct, IReadOnlyCollection<FabricWriteRequest>? fabrics, IReadOnlyCollection<AccessoryWriteRequest>? accessories, IReadOnlyCollection<int>? operationIds, int? supplierId, decimal? basePrice, PercentWriteRequest? retailPercent, PercentWriteRequest? wholesalePercent, out CreateProductCommandRequest? result)
    {
        result = null;
        if (!TryParseProductType(type, out var productType)) return false;

        var sewing = SewingProductWriteMapper.HasValues(metersPerProduct, fabrics, accessories, operationIds)
            ? SewingProductWriteMapper.Map(metersPerProduct, fabrics, accessories, operationIds)
            : null;
        var ppe = PpeProductWriteMapper.HasValues(supplierId, basePrice, retailPercent, wholesalePercent)
            ? PpeProductWriteMapper.Map(supplierId, basePrice, retailPercent, wholesalePercent)
            : null;
        result = new CreateProductCommandRequest(id, name, ruName, productType, descriptionUk, descriptionRu, categoryIds!, photos?.Select(x => new ProductPhotoRequest(x.MediaFileId, x.Alt, x.IsVisible, x.IsMain, x.SortOrder)).ToArray()!, informationBlocks?.Select(x => new InformationBlockRequest(x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder)).ToArray()!, characteristicTables?.Select(x => new CharacteristicTableRequest(x.TitleUk, x.TitleRu, x.SortOrder, x.Rows?.Select(row => new CharacteristicRowRequest(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder)).ToArray()!)).ToArray()!, sewing, ppe);
        return true;
    }

    private static bool TryParseProductType(string type, out ProductType productType)
    {
        productType = type switch { "Sewing" => ProductType.Sewing, "Ppe" => ProductType.Ppe, _ => default };
        return type is "Sewing" or "Ppe";
    }

}
