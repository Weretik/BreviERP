using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Specifications;
using Catalog.Domain.Media.Entities;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.ProductCategories.ValueObjects;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Create;

public sealed class CreateProductCommandHandler(
    ICatalogRepository<ProductEntity> repository,
    ICatalogReadRepository<MediaFile> mediaRepository,
    IProductReferenceReader referenceReader,
    IProductSlugGenerator slugGenerator)
    : ICommandHandler<CreateProductCommand, Result<ProductAdminDetail>>
{
    public async ValueTask<Result<ProductAdminDetail>> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var name = request.Name.Trim();
        var ruName = request.RuName.Trim();
        var slug = slugGenerator.GenerateFromUkrainianName(name);
        var exists = await repository.AnyAsync(new ProductByIdOrNameSpec(request.Id, name, ruName, slug), cancellationToken);
        if (exists)
            return Result.Conflict("Product ID, Ukrainian name, Russian name, or slug already exists.");

        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var missingReference = FindMissingReference(request, references);
        if (missingReference is not null)
            return Result.Conflict(missingReference);

        var now = TimeProvider.System.GetUtcNow();
        var product = ProductEntity.Create(ProductId.Create(request.Id), name, ruName, ProductSlug.Create(slug), request.Type, now);
        product.SetDescriptions(request.DescriptionUk, request.DescriptionRu, now);
        product.ReplaceCategories(request.CategoryIds.Select(ProductCategoryId.Create), now);

        foreach (var photo in request.Photos)
        {
            var media = await mediaRepository.FirstOrDefaultAsync(new Features.Media.Shared.Specifications.MediaFileByIdSpec(photo.MediaFileId), cancellationToken);
            if (media is null || !media.IsReadyForProductUsage())
                return Result.Conflict($"Media file {photo.MediaFileId} is not ready for Product usage.");
            // Photo IDs are local to a Product (the persistence key is ProductId + PhotoId).
            product.AddPhoto(
                ProductPhotoId.Create(Random.Shared.Next(1, 1_000_000_001)),
                MediaFileId.Create(photo.MediaFileId),
                now,
                photo.Alt,
                photo.IsVisible,
                photo.SortOrder,
                photo.IsMain);
        }

        var blocks = request.InformationBlocks.Select(x => new ProductInformationBlock(product.Id, x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder));
        var tables = request.CharacteristicTables.Select(x =>
        {
            var table = new ProductCharacteristicTable(product.Id, x.TitleUk, x.TitleRu, x.SortOrder, []);
            foreach (var row in x.Rows) table.AddRow(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder);
            return table;
        });
        product.ReplaceContent(blocks, tables, now);

        if (request.Type == ProductType.Sewing)
        {
            var sewing = request.Sewing!;
            product.ConfigureSewing(SewingProductDetails.Create(product.Id, sewing.MetersPerProduct,
                sewing.Fabrics.Select(x => new ProductFabric(product.Id, x.FabricId, x.IsPrimary, x.SortOrder)),
                sewing.Accessories.Select(x => new ProductAccessory(product.Id, x.GarmentAccessoryId, x.Quantity, x.SortOrder)),
                sewing.OperationIds.Select(x => new ProductOperation(product.Id, x))), now);
        }
        else
        {
            var ppe = request.Ppe!;
            product.ConfigurePpe(PpeProductDetails.Create(product.Id, ppe.SupplierId, ppe.BasePrice,
                ToRetailPercent(ppe.RetailPercent), ToWholesalePercent(ppe.WholesalePercent)), now);
        }
        await repository.AddAsync(product, cancellationToken);
        return Result.Success(ProductAdminDetailMapper.Map(product));
    }

    private static string? FindMissingReference(CreateProductCommandRequest request, ProductReferenceData references)
    {
        if (request.Type == ProductType.Sewing)
        {
            var sewing = request.Sewing!;
            if (sewing.Fabrics.Any(x => !references.Fabrics.ContainsKey(x.FabricId))) return "One or more Fabric IDs do not exist.";
            if (sewing.Accessories.Any(x => !references.GarmentAccessories.ContainsKey(x.GarmentAccessoryId))) return "One or more GarmentAccessory IDs do not exist.";
            if (sewing.OperationIds.Any(x => !references.GarmentPartOperations.ContainsKey(x))) return "One or more GarmentPartOperation IDs do not exist.";
            return null;
        }
        var ppe = request.Ppe!;
        if (!references.Suppliers.ContainsKey(ppe.SupplierId)) return "Supplier ID does not exist.";
        return ValidatePercent(ppe.RetailPercent, references) ?? ValidatePercent(ppe.WholesalePercent, references);
    }

    private static string? ValidatePercent(PercentRequest percent, ProductReferenceData references)
    {
        if (percent.Source == PricePercentSource.Custom) return percent.CustomPercent is >= 0 && percent.AdditionalReferenceId is null ? null : "Custom percent is invalid.";
        if (percent.Source != PricePercentSource.Reference || !percent.AdditionalReferenceId.HasValue || percent.CustomPercent.HasValue ||
            !references.AdditionalReferences.TryGetValue(percent.AdditionalReferenceId.Value, out var item) || item.Unit != "%")
            return "AdditionalReference percent must exist and use '%' unit.";
        return null;
    }

    private static RetailPricePercent ToRetailPercent(PercentRequest value) => value.Source == PricePercentSource.Reference
        ? RetailPricePercent.FromReference(value.AdditionalReferenceId!.Value) : RetailPricePercent.FromCustom(value.CustomPercent!.Value);
    private static WholesalePricePercent ToWholesalePercent(PercentRequest value) => value.Source == PricePercentSource.Reference
        ? WholesalePricePercent.FromReference(value.AdditionalReferenceId!.Value) : WholesalePricePercent.FromCustom(value.CustomPercent!.Value);
}
