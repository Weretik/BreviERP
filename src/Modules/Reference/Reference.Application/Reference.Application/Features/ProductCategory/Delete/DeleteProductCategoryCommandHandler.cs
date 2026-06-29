using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductCategory.Shared.Specifications;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Delete;

public sealed class DeleteProductCategoryCommandHandler(IReferenceRepository<ProductCategoryEntity> repository)
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

        await repository.DeleteAsync(entity, cancellationToken);

        return Result.Success();
    }
}
