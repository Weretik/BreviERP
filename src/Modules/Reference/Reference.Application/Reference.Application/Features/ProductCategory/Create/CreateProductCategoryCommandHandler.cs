using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductCategory.Shared.Specifications;
using Reference.Domain.AdditionalReferences.ValueObjects;
using Reference.Domain.GarmentAccessories.ValueObjects;
using Reference.Domain.GarmentPartOperations.ValueObjects;
using Reference.Domain.Products.ValueObjects;
using Reference.Domain.Suppliers.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Products.Entities.ProductCategory;

namespace Reference.Application.Features.ProductCategory.Create;

public sealed class CreateProductCategoryCommandHandler(IReferenceRepository<ProductCategoryEntity> repository)
    : ICommandHandler<CreateProductCategoryCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(
        CreateProductCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var id = ProductCategoryId.From(request.Id);
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

        var entity = ProductCategoryEntity.Create(
            id,
            name,
            ruName,
            slug,
            request.ParentId.HasValue ? ProductCategoryId.From(request.ParentId.Value) : null,
            parent?.Path,
            request.SortOrder,
            request.IsActive,
            request.Description);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}
