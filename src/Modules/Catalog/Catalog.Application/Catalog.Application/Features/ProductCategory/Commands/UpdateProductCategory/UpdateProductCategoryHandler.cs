using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.ProductCategory.Shared;

namespace Catalog.Application.Features.ProductCategory.Commands.UpdateProductCategory;

public sealed class UpdateProductCategoryHandler(ICatalogRepository<ProductCategoryEntity> repository)
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
                return Result.Conflict("Категорія не може бути батьківською для самої себе.");

            parent = await repository.FirstOrDefaultAsync(
                new ProductCategoryByIdSpec(request.ParentId.Value), cancellationToken);

            if (parent is null)
                return Result.NotFound("Батьківську категорію не знайдено.");

            if (parent.Path.StartsWith(entity.Path, StringComparison.Ordinal))
                return Result.Conflict("Категорію не можна перенести всередину її власної підкатегорії.");
        }

        var parentChanged = entity.ParentId?.Value != request.ParentId;
        var duplicateSlugExists = await repository.AnyAsync(
            new ProductCategoryByParentAndSlugExceptIdSpec(command.Id, request.ParentId, slug), cancellationToken);

        if (duplicateSlugExists)
            return Result.Conflict("Категорія з такою адресою (slug) у цьому розділі вже є.");

        var oldPath = entity.Path;
        List<ProductCategoryEntity> descendants = parentChanged
            ? await repository.ListAsync(new ProductCategoryDescendantsByPathSpec(command.Id, oldPath), cancellationToken)
            : [];

        var updated = entity.Update(
            request.Name.Trim(),
            request.RuName.Trim(),
            slug,
            request.SortOrder,
            request.IsActive,
            request.Description);
        if (updated.IsFailure) return updated.Error!.ToResult();

        if (parentChanged)
        {
            var moved = entity.MoveTo(
                request.ParentId.HasValue ? ProductCategoryId.Create(request.ParentId.Value) : null,
                parent?.Path);
            if (moved.IsFailure) return moved.Error!.ToResult();
        }

        if (parentChanged && descendants.Count > 0)
        {
            foreach (var descendant in descendants)
            {
                // A deep tree may outgrow the path limit; nothing is saved then.
                var rebased = descendant.RebasePath(oldPath, entity.Path);
                if (rebased.IsFailure) return rebased.Error!.ToResult();
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


