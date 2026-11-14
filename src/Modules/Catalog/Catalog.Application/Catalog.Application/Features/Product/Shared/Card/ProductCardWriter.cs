using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Shared.Card;

/// <summary>
/// The product card of «Створити товар» and «Зберегти товар»: descriptions, categories, photos, texts and the sewing
/// or PPE details. Each step returns the first broken rule; the caller saves nothing then.
/// </summary>
internal static class ProductCardWriter
{
    public static DomainResult Apply(ProductEntity product, CreateProductCommandRequest request, DateTimeOffset now)
    {
        var descriptions = product.SetDescriptions(request.DescriptionUk, request.DescriptionRu, now);
        if (descriptions.IsFailure) return descriptions;
        product.ReplaceCategories(request.CategoryIds.Select(ProductCategoryId.Create), now);

        foreach (var (photo, index) in request.Photos.Select((photo, index) => (photo, index)))
        {
            var added = product.AddPhoto(ProductPhotoId.Create(index + 1), MediaFileId.Create(photo.MediaFileId), now,
                photo.Alt, photo.IsVisible, photo.SortOrder, photo.IsMain);
            if (added.IsFailure) return added;
        }

        var content = Content(product.Id, request);
        if (content.IsFailure) return content.Error!;
        var replaced = product.ReplaceContent(content.Value.Blocks, content.Value.Tables, now);
        if (replaced.IsFailure) return replaced;

        return request.Type == ProductType.Sewing
            ? ConfigureSewing(product, request.Sewing!, now)
            : ConfigurePpe(product, request.Ppe!, now);
    }

    /// <summary>The reference that the request names but the reference data does not have, or null.</summary>
    public static string? FindMissingReference(CreateProductCommandRequest request, ProductReferenceData references)
    {
        if (request.Type == ProductType.Sewing)
        {
            var sewing = request.Sewing!;
            if (sewing.Fabrics.Any(x => !references.Fabrics.ContainsKey(x.FabricId)))
                return "Деяких тканин немає в довіднику.";
            if (sewing.Accessories.Any(x => !references.GarmentAccessories.ContainsKey(x.GarmentAccessoryId)))
                return "Деякої фурнітури немає в довіднику.";
            return sewing.OperationIds.Any(x => !references.GarmentPartOperations.ContainsKey(x))
                ? "Деяких операцій немає в довіднику."
                : null;
        }

        var ppe = request.Ppe!;
        if (!references.Suppliers.ContainsKey(ppe.SupplierId)) return "Постачальника немає в довіднику.";
        return MissingPercent(ppe.RetailPercent, references) ?? MissingPercent(ppe.WholesalePercent, references);
    }

    private static string? MissingPercent(PercentRequest percent, ProductReferenceData references) =>
        percent.Source == PricePercentSource.Custom
        || references.AdditionalReferences.TryGetValue(percent.AdditionalReferenceId!.Value, out var item) && item.Unit == "%"
            ? null
            : "Відсоток має бути з додаткового довідника з одиницею «%».";

    private static DomainResult<(List<ProductInformationBlock> Blocks, List<ProductCharacteristicTable> Tables)> Content(
        ProductId productId, CreateProductCommandRequest request)
    {
        var blocks = new List<ProductInformationBlock>();
        foreach (var x in request.InformationBlocks)
        {
            var block = ProductInformationBlock.Create(productId, x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder);
            if (block.IsFailure) return block.Error!;
            blocks.Add(block.Value);
        }

        var tables = new List<ProductCharacteristicTable>();
        foreach (var x in request.CharacteristicTables)
        {
            var table = ProductCharacteristicTable.Create(productId, x.TitleUk, x.TitleRu, x.SortOrder);
            if (table.IsFailure) return table.Error!;
            foreach (var row in x.Rows)
            {
                var added = table.Value.AddRow(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder);
                if (added.IsFailure) return added.Error!;
            }

            tables.Add(table.Value);
        }

        return (blocks, tables);
    }

    private static DomainResult ConfigureSewing(ProductEntity product, SewingProductRequest sewing, DateTimeOffset now)
    {
        var fabrics = new List<ProductFabric>();
        foreach (var x in sewing.Fabrics)
        {
            var fabric = ProductFabric.Create(product.Id, x.FabricId, x.IsPrimary, x.SortOrder);
            if (fabric.IsFailure) return fabric.Error!;
            fabrics.Add(fabric.Value);
        }

        var accessories = new List<ProductAccessory>();
        foreach (var x in sewing.Accessories)
        {
            var accessory = ProductAccessory.Create(product.Id, x.GarmentAccessoryId, x.Quantity, x.SortOrder);
            if (accessory.IsFailure) return accessory.Error!;
            accessories.Add(accessory.Value);
        }

        var operations = new List<ProductOperation>();
        foreach (var id in sewing.OperationIds)
        {
            var operation = ProductOperation.Create(product.Id, id);
            if (operation.IsFailure) return operation.Error!;
            operations.Add(operation.Value);
        }

        var details = SewingProductDetails.Create(product.Id, sewing.MetersPerProduct, fabrics, accessories, operations);
        return details.IsFailure ? details.Error! : product.ConfigureSewing(details.Value, now);
    }

    private static DomainResult ConfigurePpe(ProductEntity product, PpeProductRequest ppe, DateTimeOffset now)
    {
        var retail = ppe.RetailPercent.Source == PricePercentSource.Reference
            ? RetailPricePercent.FromReference(ppe.RetailPercent.AdditionalReferenceId!.Value)
            : RetailPricePercent.FromCustom(ppe.RetailPercent.CustomPercent!.Value);
        if (retail.IsFailure) return retail.Error!;
        var wholesale = ppe.WholesalePercent.Source == PricePercentSource.Reference
            ? WholesalePricePercent.FromReference(ppe.WholesalePercent.AdditionalReferenceId!.Value)
            : WholesalePricePercent.FromCustom(ppe.WholesalePercent.CustomPercent!.Value);
        if (wholesale.IsFailure) return wholesale.Error!;
        var details = PpeProductDetails.Create(product.Id, ppe.SupplierId, ppe.BasePrice, retail.Value, wholesale.Value);
        return details.IsFailure ? details.Error! : product.ConfigurePpe(details.Value, now);
    }
}
