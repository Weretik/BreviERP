using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductCategory.Shared.Specifications;
using Reference.Domain.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Update;

public sealed class UpdateProductCategoryCommandHandler(IReferenceRepository<ProductCategoryEntity> repository)
    : ICommandHandler<UpdateProductCategoryCommand, Result>
{
    public async ValueTask<Result> Handle(
        UpdateProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var entity = await repository.FirstOrDefaultAsync(
            new ProductCategoryByIdSpec(command.Id), cancellationToken);

        if (entity is null)
            return Result.NotFound();

        var request = command.Request;
        var slug = request.Slug.Trim();

        ProductCategoryEntity? parent = null;
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == command.Id)
                return Result.Conflict("Product category cannot be its own parent.");

            parent = await repository.FirstOrDefaultAsync(
                new ProductCategoryByIdSpec(request.ParentId.Value), cancellationToken);

            if (parent is null)
                return Result.NotFound("Parent product category was not found.");
        }

        var parentChanged = entity.ParentId?.Value != request.ParentId;
        if (parentChanged)
        {
            var hasChildren = await repository.AnyAsync(
                new ProductCategoryHasChildrenSpec(command.Id), cancellationToken);

            if (hasChildren)
                return Result.Conflict("Product category with children cannot be moved.");
        }

        var duplicateSlugExists = await repository.AnyAsync(
            new ProductCategoryByParentAndSlugExceptIdSpec(command.Id, request.ParentId, slug), cancellationToken);

        if (duplicateSlugExists)
            return Result.Conflict("Product category with the same parent and slug already exists.");

        entity.Update(
            request.Name.Trim(),
            request.RuName.Trim(),
            slug,
            request.SortOrder,
            request.IsActive,
            request.Description);

        if (parentChanged)
        {
            entity.MoveTo(
                request.ParentId.HasValue ? ProductCategoryId.From(request.ParentId.Value) : null,
                parent?.Path);
        }

        await repository.UpdateAsync(entity, cancellationToken);

        return Result.Success();
    }
}
