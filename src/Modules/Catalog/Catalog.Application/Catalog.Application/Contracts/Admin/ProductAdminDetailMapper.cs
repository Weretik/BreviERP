using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Contracts.Admin;

public static class ProductAdminDetailMapper
{
    public static ProductAdminDetail Map(ProductEntity product) => new(
        product.Id.Value,
        product.Name,
        product.RuName,
        product.Slug.Value,
        product.Type,
        product.DescriptionUk,
        product.DescriptionRu,
        product.Categories.Select(x => x.CategoryId.Value).ToList(),
        product.Photos.OrderBy(x => x.SortOrder).Select(x => new ProductPhotoDetail(x.MediaFileId.Value, x.Alt, x.IsVisible, x.IsMain, x.SortOrder)).ToList(),
        product.InformationBlocks.OrderBy(x => x.SortOrder).Select(x => new InformationBlockAdminDetail(x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder)).ToList(),
        product.CharacteristicTables.OrderBy(x => x.SortOrder).Select(x => new CharacteristicTableAdminDetail(x.TitleUk, x.TitleRu, x.SortOrder, x.Rows.OrderBy(r => r.SortOrder).Select(r => new CharacteristicRowAdminDetail(r.LabelUk, r.LabelRu, r.ValueUk, r.ValueRu, r.SortOrder)).ToList())).ToList(),
        product.SewingDetails is { } s ? new SewingAdminDetail(s.MetersPerProduct, s.Fabrics.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).Select(x => new FabricAdminDetail(x.FabricId, x.IsPrimary, x.SortOrder)).ToList(), s.Accessories.OrderBy(x => x.SortOrder).Select(x => new AccessoryAdminDetail(x.GarmentAccessoryId, x.Quantity, x.SortOrder)).ToList(), s.Operations.Select(x => x.GarmentPartOperationId).ToList()) : null,
        product.PpeDetails is { } p ? new PpeAdminDetail(p.SupplierId, p.BasePrice, p.RetailPercent.AdditionalReferenceId, p.RetailPercent.CustomPercent, p.WholesalePercent.AdditionalReferenceId, p.WholesalePercent.CustomPercent) : null,
        product.CreatedAt,
        product.UpdatedAt);
}
