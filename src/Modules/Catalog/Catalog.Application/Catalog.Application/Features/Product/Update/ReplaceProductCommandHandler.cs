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

namespace Catalog.Application.Features.Product.Update;

public sealed class ReplaceProductCommandHandler(
    ICatalogRepository<ProductEntity> repository,
    ICatalogReadRepository<MediaFile> mediaRepository,
    IProductReferenceReader referenceReader,
    IProductSlugGenerator slugGenerator) : ICommandHandler<ReplaceProductCommand, Result<ProductAdminDetail>>
{
    public async ValueTask<Result<ProductAdminDetail>> Handle(ReplaceProductCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductByIdSpec(command.Id), cancellationToken);
        if (product is null) return Result.NotFound();
        var request = command.Request;
        if (request.Id != command.Id) return Result.Invalid([new ValidationError("Request.Id", "Request ID must match the route ID.")]);
        if ((request.Type == ProductType.Sewing && (request.Sewing is null || request.Ppe is not null)) ||
            (request.Type == ProductType.Ppe && (request.Ppe is null || request.Sewing is not null)))
            return Result.Invalid([new ValidationError("Request.Type", "Product must contain data for its selected type only.")]);
        var name = request.Name.Trim(); var ruName = request.RuName.Trim();
        var slug = slugGenerator.GenerateFromUkrainianName(name);
        if (await repository.AnyAsync(new ProductByIdOrNameExceptIdSpec(command.Id, name, ruName, slug), cancellationToken))
            return Result.Conflict("Product Ukrainian name, Russian name, or slug already exists.");
        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var referenceError = ValidateReferences(request, references);
        if (referenceError is not null) return Result.Conflict(referenceError);
        var now = TimeProvider.System.GetUtcNow();
        product.Update(name, ruName, ProductSlug.Create(slug), request.Type, now);
        product.SetDescriptions(request.DescriptionUk, request.DescriptionRu, now);
        product.ReplaceCategories(request.CategoryIds.Select(ProductCategoryId.Create), now);
        foreach (var existing in product.Photos.ToList()) product.RemovePhoto(existing.Id, now);
        foreach (var photo in request.Photos)
        {
            var media = await mediaRepository.FirstOrDefaultAsync(new Features.Media.Shared.Specifications.MediaFileByIdSpec(photo.MediaFileId), cancellationToken);
            if (media is null || !media.IsReadyForProductUsage()) return Result.Conflict($"Media file {photo.MediaFileId} is not ready for Product usage.");
            product.AddPhoto(ProductPhotoId.Create(Random.Shared.Next(1, 1_000_000_001)), MediaFileId.Create(photo.MediaFileId), now, photo.Alt, photo.IsVisible, photo.SortOrder, photo.IsMain);
        }
        var blocks = request.InformationBlocks.Select(x => new ProductInformationBlock(product.Id, x.TitleUk, x.TitleRu, x.TextUk, x.TextRu, x.SortOrder));
        var tables = request.CharacteristicTables.Select(x => { var table = new ProductCharacteristicTable(product.Id, x.TitleUk, x.TitleRu, x.SortOrder, []); foreach (var row in x.Rows) table.AddRow(row.LabelUk, row.LabelRu, row.ValueUk, row.ValueRu, row.SortOrder); return table; });
        product.ReplaceContent(blocks, tables, now);
        if (request.Type == ProductType.Sewing)
        {
            var s = request.Sewing!;
            product.ConfigureSewing(SewingProductDetails.Create(product.Id, s.MetersPerProduct, s.Fabrics.Select(x => new ProductFabric(product.Id, x.FabricId, x.IsPrimary, x.SortOrder)), s.Accessories.Select(x => new ProductAccessory(product.Id, x.GarmentAccessoryId, x.Quantity, x.SortOrder)), s.OperationIds.Select(x => new ProductOperation(product.Id, x))), now);
        }
        else
        {
            var p = request.Ppe!;
            product.ConfigurePpe(PpeProductDetails.Create(product.Id, p.SupplierId, p.BasePrice, ToRetail(p.RetailPercent), ToWholesale(p.WholesalePercent)), now);
        }
        await repository.UpdateAsync(product, cancellationToken);
        return Result.Success(ProductAdminDetailMapper.Map(product));
    }

    private static string? ValidateReferences(CreateProductCommandRequest r, ProductReferenceData d)
    {
        if (r.Type == ProductType.Sewing) { var s = r.Sewing!; if (s.Fabrics.Any(x => !d.Fabrics.ContainsKey(x.FabricId))) return "One or more Fabric IDs do not exist."; if (s.Accessories.Any(x => !d.GarmentAccessories.ContainsKey(x.GarmentAccessoryId))) return "One or more GarmentAccessory IDs do not exist."; return s.OperationIds.Any(x => !d.GarmentPartOperations.ContainsKey(x)) ? "One or more GarmentPartOperation IDs do not exist." : null; }
        var p = r.Ppe!; if (!d.Suppliers.ContainsKey(p.SupplierId)) return "Supplier ID does not exist."; return ValidPercent(p.RetailPercent, d) ?? ValidPercent(p.WholesalePercent, d);
    }
    private static string? ValidPercent(PercentRequest p, ProductReferenceData d) => p.Source == PricePercentSource.Custom ? p.CustomPercent is >= 0 && p.AdditionalReferenceId is null ? null : "Custom percent is invalid." : p.Source == PricePercentSource.Reference && p.AdditionalReferenceId.HasValue && !p.CustomPercent.HasValue && d.AdditionalReferences.TryGetValue(p.AdditionalReferenceId.Value, out var a) && a.Unit == "%" ? null : "AdditionalReference percent must exist and use '%' unit.";
    private static RetailPricePercent ToRetail(PercentRequest p) => p.Source == PricePercentSource.Reference ? RetailPricePercent.FromReference(p.AdditionalReferenceId!.Value) : RetailPricePercent.FromCustom(p.CustomPercent!.Value);
    private static WholesalePricePercent ToWholesale(PercentRequest p) => p.Source == PricePercentSource.Reference ? WholesalePricePercent.FromReference(p.AdditionalReferenceId!.Value) : WholesalePricePercent.FromCustom(p.CustomPercent!.Value);
}
