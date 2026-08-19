using Catalog.Application.Contracts.Admin.Ppe;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Contracts.Admin.Sewing;

namespace Catalog.Api.Contracts.Products;

public static class ProductDetailResponseMapper
{
    public static ProductDetailResponse ToResponse(ProductAdminDetail detail)
        => new(detail.Id, detail.Name, detail.RuName, detail.Slug, detail.Type.ToString(),
            detail.Categories.Select(x => x.Id).ToList(),
            detail.Photos.Where(x => x.IsMain).Select(x => new ProductMainPhotoResponse(x.MediaFileId, x.Url)).FirstOrDefault(),
            CalculateMinimumWholesalePrice(detail), detail.DescriptionUk, detail.DescriptionRu,
            detail.Categories.Select(x => new CategoryResponse(x.Id, x.Name, x.RuName, x.Slug)).ToList(),
            detail.Photos.Select(x => new ProductPhotoResponse(x.MediaFileId, x.Url, x.Alt, x.IsVisible, x.IsMain, x.SortOrder)).ToList(),
            detail.InformationBlocks.Select(x => new InformationBlockResponse(x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder)).ToList(),
            detail.CharacteristicTables.Select(x => new CharacteristicTableResponse(x.TitleUk, x.TitleRu, x.SortOrder,
                x.Rows.Select(row => new CharacteristicRowResponse(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder)).ToList())).ToList(),
            detail.Sewing is null ? null : ToResponse(detail.Sewing), detail.Ppe is null ? null : ToResponse(detail.Ppe),
            detail.CreatedAtUtc, detail.UpdatedAtUtc);

    private static decimal CalculateMinimumWholesalePrice(ProductAdminDetail detail)
        => detail.Sewing?.Prices?.ByFabric.SelectMany(x => new[] { x.Price1To10, x.Price11To39, x.Price40Plus }).DefaultIfEmpty(0m).Min()
            ?? detail.Ppe?.WholesalePrice
            ?? 0m;

    private static SewingResponse ToResponse(SewingAdminDetail sewing)
        => new(sewing.MetersPerProduct,
            sewing.Fabrics.Select(x => new FabricResponse(x.FabricId, x.Name, x.Price, x.IsPrimary, x.SortOrder)).ToList(),
            sewing.Accessories.Select(x => new AccessoryResponse(x.GarmentAccessoryId, x.Name, x.Price, x.Quantity, x.SortOrder)).ToList(),
            sewing.Operations.Select(x => new OperationResponse(x.Id, x.Name, x.Minutes)).ToList(), sewing.PiecesPerShift,
            sewing.Prices is null ? null : ToResponse(sewing.Prices));

    private static SewingPricesResponse ToResponse(SewingPrices prices)
        => new(prices.ByFabric.Select(x => new SewingFabricPricesResponse(x.FabricId, x.Price1To10, x.Price11To39, x.Price40Plus)).ToList(),
            new SewingPriceRangesResponse(ToResponse(prices.Ranges.Price1To10), ToResponse(prices.Ranges.Price11To39), ToResponse(prices.Ranges.Price40Plus)));

    private static SewingPriceRangeResponse ToResponse(SewingPriceRange range)
        => new(range.MinPrice, range.MinFabricId, range.MaxPrice, range.MaxFabricId);

    private static PpeResponse ToResponse(PpeAdminDetail ppe)
        => new(new NamedReferenceResponse(ppe.Supplier.Id, ppe.Supplier.Name), ppe.BasePrice,
            ToResponse(ppe.RetailPercent), ToResponse(ppe.WholesalePercent), ppe.RetailPrice, ppe.WholesalePrice);

    private static PpePercentResponse ToResponse(PpePercentAdminDetail percent)
        => new(percent.Source.ToString(),
            percent.Reference is null ? null : new AdditionalReferenceResponse(percent.Reference.Id, percent.Reference.Name, percent.Reference.Key, percent.Reference.Value, percent.Reference.Unit),
            percent.CustomPercent);
}
