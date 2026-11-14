using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Contracts.SlugGeneration;
using Catalog.Application.Features.Product.Create.DTOs;
using ProductAdminDetailSpec = Catalog.Application.Features.Product.GetAdminDetail.Specifications.ProductAdminDetailSpec;
using Catalog.Application.Features.Product.Shared.Card;
using Catalog.Application.Features.Product.Update.Specifications;
using Catalog.Application.Contracts.Media;
using Catalog.Domain.Media.ValueObjects;
using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;
using Catalog.Domain.Products.Entities;
using Catalog.Domain.Products.Enums;
using Catalog.Domain.Products.ValueObjects;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.Update;

public sealed class ReplaceProductCommandHandler(
    ICatalogRepository<ProductEntity> repository,
    IProductMediaReader mediaReader,
    IProductReferenceReader referenceReader,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository,
    IProductSlugGenerator slugGenerator) : ICommandHandler<ReplaceProductCommand, Result<ProductAdminDetail>>
{
    public async ValueTask<Result<ProductAdminDetail>> Handle(ReplaceProductCommand command, CancellationToken cancellationToken)
    {
        var product = await repository.FirstOrDefaultAsync(new ProductByIdSpec(command.Id), cancellationToken);
        if (product is null) return Result.NotFound();
        var request = command.Request;
        if (product.Type == ProductType.Sewing && request.Type == ProductType.Ppe &&
            (product.Articles.Count != 0 || product.MaterialLines.Count != 0 || product.OperationLines.Count != 0))
            return Result.Conflict("У товару є варіанти або склад пошиття, тому його не можна змінити на ЗІЗ.");
        if (product.MaterialLines.Count != 0 || product.OperationLines.Count != 0)
        {
            if (request.Type != ProductType.Sewing || request.Sewing is null || !LegacySewingNormsMatch(product.SewingDetails, request.Sewing))
                return Result.Conflict("Старі норми пошиття не можна змінити, поки заповнено склад. Змініть склад моделі.");
        }
        var name = request.Name.Trim(); var ruName = request.RuName.Trim();
        var slug = slugGenerator.GenerateFromUkrainianName(name);
        if (await repository.AnyAsync(new ProductByIdOrNameExceptIdSpec(command.Id, name, ruName, slug), cancellationToken))
            return Result.Conflict("Товар із такою українською чи російською назвою або адресою (slug) вже є.");
        var categories = await categoryRepository.ListAsync(
            new ProductCategoriesByIdsSpec(request.CategoryIds), cancellationToken);
        if (categories.Count != request.CategoryIds.Distinct().Count())
            return Result.Conflict("Деяких категорій товару не знайдено.");
        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var referenceError = ProductCardWriter.FindMissingReference(request, references);
        if (referenceError is not null) return Result.Conflict(referenceError);
        var mediaUrls = await mediaReader.GetUrlsAsync(request.Photos.Select(x => x.MediaFileId).ToList(), cancellationToken);
        var missingPhoto = request.Photos.FirstOrDefault(x => !mediaUrls.ContainsKey(x.MediaFileId));
        if (missingPhoto is not null)
            return Result.Conflict($"Фото {missingPhoto.MediaFileId} не знайдено в медіатеці.");

        var now = TimeProvider.System.GetUtcNow();
        var productSlug = ProductSlug.Create(slug);
        if (productSlug.IsFailure) return productSlug.Error!.ToResult<ProductAdminDetail>();
        var updated = product.Update(name, ruName, productSlug.Value, request.Type, now);
        if (updated.IsFailure) return updated.Error!.ToResult<ProductAdminDetail>();
        // The photos are replaced as a whole; every one of them is in the list, so removing cannot fail.
        foreach (var existing in product.Photos.ToList()) product.RemovePhoto(existing.Id, now).ThrowIfFailure();
        var card = ProductCardWriter.Apply(product, request, now);
        if (card.IsFailure) return card.Error!.ToResult<ProductAdminDetail>();
        await repository.UpdateAsync(product, cancellationToken);
        var categoryDetails = categories.ToDictionary(
            x => x.Id.Value,
            x => new CategoryAdminDetail(x.Id.Value, x.Name, x.RuName, x.Slug));
        var detail = await repository.FirstOrDefaultAsync(new ProductAdminDetailSpec(product.Id.Value), cancellationToken);
        return detail is null
            ? Result.Error("Не вдалося прочитати збережений товар.")
            : Result.Success(ProductAdminDetailMapper.Map(detail, references, categoryDetails, mediaUrls));
    }

    private static bool LegacySewingNormsMatch(SewingProductDetails? details, SewingProductRequest request)
    {
        if (details is null || details.MetersPerProduct != request.MetersPerProduct ||
            details.Fabrics.Count != request.Fabrics.Count || details.Accessories.Count != request.Accessories.Count ||
            details.Operations.Count != request.OperationIds.Count) return false;
        return details.Fabrics.All(x => request.Fabrics.Any(y =>
                   x.FabricId == y.FabricId && x.IsPrimary == y.IsPrimary && x.SortOrder == y.SortOrder)) &&
               details.Accessories.All(x => request.Accessories.Any(y =>
                   x.GarmentAccessoryId == y.GarmentAccessoryId && x.Quantity == y.Quantity && x.SortOrder == y.SortOrder)) &&
               details.Operations.All(x => request.OperationIds.Contains(x.GarmentPartOperationId));
    }
}
