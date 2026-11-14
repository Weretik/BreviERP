using Catalog.Application.Features.Product.Shared.Card;
using ProductAdminDetailSpec = Catalog.Application.Features.Product.Queries.GetAdminProductDetail.ProductAdminDetailSpec;
using Catalog.Domain.Media;
using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;
using Catalog.Domain.Products;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Shared;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Commands.CreateProduct;

public sealed class CreateProductHandler(
    ICatalogRepository<ProductEntity> repository,
    IProductMediaReader mediaReader,
    IProductReferenceReader referenceReader,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository,
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
            return Result.Conflict("Товар із таким номером, українською чи російською назвою або адресою (slug) вже є.");

        var categories = await categoryRepository.ListAsync(
            new ProductCategoriesByIdsSpec(request.CategoryIds), cancellationToken);
        if (categories.Count != request.CategoryIds.Distinct().Count())
            return Result.Conflict("Деяких категорій товару не знайдено.");

        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var missingReference = ProductCardWriter.FindMissingReference(request, references);
        if (missingReference is not null)
            return Result.Conflict(missingReference);

        var mediaUrls = await mediaReader.GetUrlsAsync(request.Photos.Select(x => x.MediaFileId).ToList(), cancellationToken);
        var missingPhoto = request.Photos.FirstOrDefault(x => !mediaUrls.ContainsKey(x.MediaFileId));
        if (missingPhoto is not null)
            return Result.Conflict($"Фото {missingPhoto.MediaFileId} не знайдено в медіатеці.");

        var now = TimeProvider.System.GetUtcNow();
        var productSlug = ProductSlug.Create(slug);
        if (productSlug.IsFailure) return productSlug.Error!.ToResult<ProductAdminDetail>();
        var created = ProductEntity.Create(ProductId.Create(request.Id), name, ruName, productSlug.Value, request.Type, now);
        if (created.IsFailure) return created.Error!.ToResult<ProductAdminDetail>();
        var product = created.Value;
        var card = ProductCardWriter.Apply(product, request, now);
        if (card.IsFailure) return card.Error!.ToResult<ProductAdminDetail>();
        await repository.AddAsync(product, cancellationToken);
        var categoryDetails = categories.ToDictionary(
            x => x.Id.Value,
            x => new CategoryAdminDetail(x.Id.Value, x.Name, x.RuName, x.Slug));
        var detail = await repository.FirstOrDefaultAsync(new ProductAdminDetailSpec(product.Id.Value), cancellationToken);
        return detail is null
            ? Result.Error("Не вдалося прочитати створений товар.")
            : Result.Success(ProductAdminDetailMapper.Map(detail, references, categoryDetails, mediaUrls));
    }
}
