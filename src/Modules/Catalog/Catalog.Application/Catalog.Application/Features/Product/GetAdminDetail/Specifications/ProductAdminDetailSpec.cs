using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminDetail.Specifications;

public sealed class ProductAdminDetailSpec : Specification<ProductEntity, ProductAdminDetailReadModel>
{
    public ProductAdminDetailSpec(int id)
    {
        Query.AsNoTracking()
            .Where(x => x.Id == Catalog.Domain.Products.ValueObjects.ProductId.Create(id))
            .Select(x => new ProductAdminDetailReadModel(
                x.Id.Value,
                x.Name,
                x.RuName,
                x.Slug.Value,
                x.Type,
                x.DescriptionUk,
                x.DescriptionRu,
                x.Categories.Select(category => category.CategoryId.Value).ToList(),
                x.Photos.Select(photo => new ProductPhotoReadModel(
                    photo.MediaFileId.Value, photo.Alt, photo.IsVisible, photo.IsMain, photo.SortOrder)).ToList(),
                x.InformationBlocks.Select(block => new InformationBlockReadModel(
                    block.TitleUk, block.TitleRu, block.TextUk, block.TextRu, block.SortOrder)).ToList(),
                x.CharacteristicTables.Select(table => new CharacteristicTableReadModel(
                    table.TitleUk, table.TitleRu, table.SortOrder,
                    table.Rows.Select(row => new CharacteristicRowReadModel(
                        row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder)).ToList())).ToList(),
                x.SewingDetails == null ? null : new SewingReadModel(
                    x.SewingDetails.MetersPerProduct,
                    x.SewingDetails.Fabrics.Select(fabric => new FabricReadModel(
                        fabric.FabricId, fabric.IsPrimary, fabric.SortOrder)).ToList(),
                    x.SewingDetails.Accessories.Select(accessory => new AccessoryReadModel(
                        accessory.GarmentAccessoryId, accessory.Quantity, accessory.SortOrder)).ToList(),
                    x.SewingDetails.Operations.Select(operation => operation.GarmentPartOperationId).ToList()),
                x.PpeDetails == null ? null : new PpeReadModel(
                    x.PpeDetails.SupplierId,
                    x.PpeDetails.BasePrice,
                    x.PpeDetails.RetailPercent.Source,
                    x.PpeDetails.RetailPercent.AdditionalReferenceId,
                    x.PpeDetails.RetailPercent.CustomPercent,
                    x.PpeDetails.WholesalePercent.Source,
                    x.PpeDetails.WholesalePercent.AdditionalReferenceId,
                    x.PpeDetails.WholesalePercent.CustomPercent),
                x.CreatedAt,
                x.UpdatedAt));
    }
}
