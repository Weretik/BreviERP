using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.Admin.Ppe;
using Catalog.Application.Contracts.Admin.Reference;
using Catalog.Application.Contracts.Admin.Sewing;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;

namespace Catalog.Application.Contracts.Admin.Product;

public static class ProductAdminDetailMapper
{
    public static ProductAdminDetail Map(
        ProductAdminDetailReadModel product,
        ProductReferenceData references,
        IReadOnlyDictionary<int, CategoryAdminDetail> categories,
        IReadOnlyDictionary<int, string> mediaUrls) => new(
        product.Id,
        product.Name,
        product.RuName,
        product.Slug,
        product.Type,
        product.DescriptionUk,
        product.DescriptionRu,
        product.CategoryIds.Select(id => categories.GetValueOrDefault(id)).OfType<CategoryAdminDetail>().ToList(),
        product.Photos.OrderBy(x => x.SortOrder)
            .Where(x => mediaUrls.ContainsKey(x.MediaFileId))
            .Select(x => new ProductPhotoDetail(x.MediaFileId, mediaUrls[x.MediaFileId], x.Alt, x.IsVisible, x.IsMain, x.SortOrder)).ToList(),
        product.InformationBlocks.OrderBy(x => x.SortOrder).Select(x => new InformationBlockAdminDetail(x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder)).ToList(),
        product.CharacteristicTables.OrderBy(x => x.SortOrder).Select(table => new CharacteristicTableAdminDetail(
            table.TitleUk, table.TitleRu, table.SortOrder, table.Rows.OrderBy(row => row.SortOrder)
                .Select(row => new CharacteristicRowAdminDetail(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder)).ToList())).ToList(),
        product.Sewing is null ? null : SewingAdminDetailMapper.Map(product.Sewing, references),
        product.Ppe is null ? null : PpeAdminDetailMapper.Map(product.Ppe, references),
        product.CreatedAtUtc,
        product.UpdatedAtUtc);

}
