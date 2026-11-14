using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.Shared.Specifications;
using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Update;

public sealed class UpdateProductCategoryCommandHandler(ICatalogRepository<ProductCategoryEntity> repository)
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
                request.ParentId.HasValue ? ProductCategoryId.Create(request.ParentId.Value) : null,
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


