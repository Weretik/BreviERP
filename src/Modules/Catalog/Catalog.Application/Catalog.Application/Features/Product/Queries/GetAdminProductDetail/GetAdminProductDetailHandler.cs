using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;
using ProductEntity = Catalog.Domain.Products.Product;
using Catalog.Application.Abstractions;
using Catalog.Application.Abstractions.Media;
using Catalog.Application.Abstractions.Reference;
using Catalog.Application.Features.Product.Shared;
using Catalog.Application.Features.Product.Queries.GetAdminProductDetail.Specs;
using Catalog.Application.Features.Product.Shared.Detail;

namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed class GetAdminProductDetailHandler(
    ICatalogReadRepository<ProductEntity> productRepository,
    IProductReferenceReader referenceReader,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository,
    IProductMediaReader mediaReader)
    : IQueryHandler<GetAdminProductDetailQuery, Result<ProductAdminDetail>>
{
    public async ValueTask<Result<ProductAdminDetail>> Handle(GetAdminProductDetailQuery query, CancellationToken cancellationToken)
    {
        var product = await productRepository.FirstOrDefaultAsync(new ProductAdminDetailSpec(query.Id), cancellationToken);
        if (product is null) return Result.NotFound();
        var references = await referenceReader.GetSnapshotAsync(cancellationToken);
        var categories = await categoryRepository.ListAsync(
            new ProductCategoriesByIdsSpec(product.CategoryIds), cancellationToken);
        var categoryDetails = categories.ToDictionary(
            x => x.Id.Value,
            x => new CategoryAdminDetail(x.Id.Value, x.Name, x.RuName, x.Slug));
        var mediaFileIds = product.Photos.Select(x => x.MediaFileId).Distinct().ToArray();
        var mediaUrls = mediaFileIds.Length == 0
            ? new Dictionary<int, string>()
            : await mediaReader.GetUrlsAsync(mediaFileIds, cancellationToken);
        return Result.Success(ProductAdminDetailMapper.Map(product, references, categoryDetails, mediaUrls));
    }
}
