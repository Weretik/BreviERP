using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Contracts.Admin;
using Catalog.Application.Contracts.Admin.Product;
using Catalog.Application.Contracts.Reference;
using Catalog.Application.Features.Product.GetAdminDetail.DTOs;
using Catalog.Application.Features.Product.GetAdminDetail.Specifications;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;
using ProductEntity = Catalog.Domain.Products.Entities.Product;

namespace Catalog.Application.Features.Product.GetAdminDetail;

public sealed class GetAdminProductDetailQueryHandler(
    ICatalogReadRepository<ProductEntity> productRepository,
    IProductReferenceReader referenceReader,
    ICatalogReadRepository<ProductCategoryEntity> categoryRepository)
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
        return Result.Success(ProductAdminDetailMapper.Map(product, references, categoryDetails));
    }
}
