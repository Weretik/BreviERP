using Catalog.Domain.ProductCategories;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.ProductCategory;
using Catalog.Application.Abstractions;
using Catalog.Application.Features.ProductCategory.Shared;

namespace Catalog.Application.Features.ProductCategory.Commands.CreateProductCategory;

public sealed class CreateProductCategoryHandler(ICatalogRepository<ProductCategoryEntity> repository)
    : ICommandHandler<CreateProductCategoryCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var id = ProductCategoryId.Create(request.Id);
        var name = request.Name.Trim();
        var ruName = request.RuName.Trim();
        var slug = request.Slug.Trim();

        var idExists = await repository.AnyAsync(new ProductCategoryByIdSpec(request.Id), cancellationToken);
        var nameExists = await repository.AnyAsync(new ProductCategoryByNameSpec(name), cancellationToken);

        if (idExists || nameExists)
        {
            var validationErrors = new List<ValidationError>();

            if (idExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Id",
                    "Категорія товарів з таким ідентифікатором уже існує."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "Категорія товарів з такою назвою уже існує."));
            }

            return Result.Invalid(validationErrors);
        }

        ProductCategoryEntity? parent = null;
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
            {
                return Result.Invalid([new ValidationError(
                    "Request.ParentId",
                    "Категорія товарів не може бути власною батьківською категорією.")]);
            }

            parent = await repository.FirstOrDefaultAsync(
                new ProductCategoryByIdSpec(request.ParentId.Value), cancellationToken);

            if (parent is null)
            {
                return Result.Invalid([new ValidationError(
                    "Request.ParentId",
                    "Батьківську категорію товарів не знайдено.")]);
            }
        }

        var duplicateSlugExists = await repository.AnyAsync(
            new ProductCategoryByParentAndSlugSpec(request.ParentId, slug), cancellationToken);

        if (duplicateSlugExists)
        {
            return Result.Invalid([new ValidationError(
                "Request.Slug",
                "Категорія товарів з таким slug уже існує в цій батьківській категорії.")]);
        }

        var created = ProductCategoryEntity.Create(
            id,
            name,
            ruName,
            slug,
            request.ParentId.HasValue ? ProductCategoryId.Create(request.ParentId.Value) : null,
            parent?.Path,
            request.SortOrder,
            request.IsActive,
            request.Description);
        if (created.IsFailure) return created.Error!.ToResult<int>();
        var entity = created.Value;

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}


