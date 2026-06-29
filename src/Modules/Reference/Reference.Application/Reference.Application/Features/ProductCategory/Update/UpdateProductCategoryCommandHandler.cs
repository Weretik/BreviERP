using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductCategory.Shared.Specifications;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

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

            if (parent.Path.StartsWith(entity.Path, StringComparison.Ordinal))
                return Result.Conflict("Product category cannot be moved under its own descendant.");
        }

        var parentChanged = entity.ParentId?.Value != request.ParentId;
        var duplicateSlugExists = await repository.AnyAsync(
            new ProductCategoryByParentAndSlugExceptIdSpec(command.Id, request.ParentId, slug), cancellationToken);

        if (duplicateSlugExists)
            return Result.Conflict("Product category with the same parent and slug already exists.");

        var oldPath = entity.Path;
        List<ProductCategoryEntity> descendants = parentChanged
            ? await repository.ListAsync(new ProductCategoryDescendantsByPathSpec(command.Id, oldPath), cancellationToken)
            : [];

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

        if (parentChanged && descendants.Count > 0)
        {
            foreach (var descendant in descendants)
            {
                descendant.RebasePath(oldPath, entity.Path);
            }

            descendants.Insert(0, entity);
            await repository.UpdateRangeAsync(descendants, cancellationToken);
        }
        else
        {
            await repository.UpdateAsync(entity, cancellationToken);
        }

        return Result.Success();
    }
}
