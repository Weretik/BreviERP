using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.ProductCategory.Shared;

namespace Catalog.Application.Features.ProductCategory.Commands.DeleteProductCategory;

public sealed class DeleteProductCategoryHandler(
    ICatalogRepository<ProductCategoryEntity> repository,
    ICatalogReadRepository<Catalog.Domain.Products.Product> productRepository)
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
            return Result.Conflict("Категорію з підкатегоріями видалити не можна.");

        var isUsedByProduct = await productRepository.AnyAsync(
            new ProductCategoryIsUsedByProductSpec(command.Id), cancellationToken);

        if (isUsedByProduct)
            return Result.Conflict("Категорію, до якої прив'язані товари, видалити не можна.");

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}


