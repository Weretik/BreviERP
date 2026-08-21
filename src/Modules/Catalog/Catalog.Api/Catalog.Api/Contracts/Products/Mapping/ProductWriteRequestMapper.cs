using Ardalis.Result;
using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Domain.Products.Enums;

namespace Catalog.Api.Contracts.Products;

public static class ProductWriteRequestMapper
{
    public static Result<CreateProductCommandRequest> Map(CreateProductRequest request)
        => Map(request.Id, request.Type, request.Name, request.RuName, request.DescriptionUk, request.DescriptionRu, request.CategoryIds, request.Photos, request.InformationBlocks, request.CharacteristicTables, request.MetersPerProduct, request.Fabrics, request.Accessories, request.OperationIds, request.SupplierId, request.BasePrice, request.RetailPercent, request.WholesalePercent);

    public static Result<CreateProductCommandRequest> Map(ProductWriteRequest request)
        => Map(0, request.Type, request.Name, request.RuName, request.DescriptionUk, request.DescriptionRu, request.CategoryIds, request.Photos, request.InformationBlocks, request.CharacteristicTables, request.MetersPerProduct, request.Fabrics, request.Accessories, request.OperationIds, request.SupplierId, request.BasePrice, request.RetailPercent, request.WholesalePercent);

    private static Result<CreateProductCommandRequest> Map(int id, string type, string name, string ruName, string descriptionUk, string descriptionRu, IReadOnlyCollection<int>? categoryIds, IReadOnlyCollection<ProductPhotoWriteRequest>? photos, IReadOnlyCollection<InformationBlockWriteRequest>? informationBlocks, IReadOnlyCollection<CharacteristicTableWriteRequest>? characteristicTables, decimal? metersPerProduct, IReadOnlyCollection<FabricWriteRequest>? fabrics, IReadOnlyCollection<AccessoryWriteRequest>? accessories, IReadOnlyCollection<int>? operationIds, int? supplierId, decimal? basePrice, PercentWriteRequest? retailPercent, PercentWriteRequest? wholesalePercent)
    {
        var validationErrors = new List<ValidationError>();
        var hasProductType = TryParseProductType(type, out var productType);
        if (!hasProductType)
            validationErrors.Add(new ValidationError("type", "type must be either Sewing or Ppe."));

        var sewing = SewingProductWriteMapper.HasValues(metersPerProduct, fabrics, accessories, operationIds)
            ? SewingProductWriteMapper.Map(metersPerProduct, fabrics, accessories, operationIds)
            : null;
        var ppeMapping = PpeProductWriteMapper.HasValues(supplierId, basePrice, retailPercent, wholesalePercent)
            ? PpeProductWriteMapper.Map(supplierId, basePrice, retailPercent, wholesalePercent)
            : null;
        if (ppeMapping is not null)
            validationErrors.AddRange(ppeMapping.ValidationErrors);

        if (validationErrors.Count > 0)
            return Result.Invalid(validationErrors);

        return Result.Success(new CreateProductCommandRequest(id, name, ruName, productType, descriptionUk, descriptionRu, categoryIds!, photos?.Select(x => new ProductPhotoRequest(x.MediaFileId, x.Alt, x.IsVisible, x.IsMain, x.SortOrder)).ToArray()!, informationBlocks?.Select(x => new InformationBlockRequest(x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder)).ToArray()!, characteristicTables?.Select(x => new CharacteristicTableRequest(x.TitleUk, x.TitleRu, x.SortOrder, x.Rows?.Select(row => new CharacteristicRowRequest(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder)).ToArray()!)).ToArray()!, sewing, ppeMapping?.Value));
    }

    private static bool TryParseProductType(string type, out ProductType productType)
    {
        productType = type switch { "Sewing" => ProductType.Sewing, "Ppe" => ProductType.Ppe, _ => default };
        return type is "Sewing" or "Ppe";
    }

}
