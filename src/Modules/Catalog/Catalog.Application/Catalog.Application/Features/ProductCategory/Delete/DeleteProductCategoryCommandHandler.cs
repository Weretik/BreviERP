using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.Shared.Specifications;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Delete;

public sealed class DeleteProductCategoryCommandHandler(
    ICatalogRepository<ProductCategoryEntity> repository,
    ICatalogReadRepository<Catalog.Domain.Products.Entities.Product> productRepository)
    : ICommandHandler<DeleteProductCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        DeleteProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new ProductCategoryByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var hasChildren = await repository.AnyAsync(
            new ProductCategoryHasChildrenSpec(command.Id), cancellationToken);

        if (hasChildren)
            return Result.Conflict("Product category with children cannot be deleted.");

        var isUsedByProduct = await productRepository.AnyAsync(
            new ProductCategoryIsUsedByProductSpec(command.Id), cancellationToken);

        if (isUsedByProduct)
            return Result.Conflict("Product category assigned to products cannot be deleted.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}


