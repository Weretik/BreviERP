using Reference.Application.Contracts.Persistence;
using Reference.Application.Features.ProductCategory.Shared.Specifications;
using Reference.Domain.ValueObjects;
using ProductCategoryEntity = Reference.Domain.Entities.ProductCategory;

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
        if (idExists)
            return Result.Conflict("Product category with the same id already exists.");

        ProductCategoryEntity? parent = null;
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == request.Id)
                return Result.Conflict("Product category cannot be its own parent.");

            parent = await repository.FirstOrDefaultAsync(
                new ProductCategoryByIdSpec(request.ParentId.Value), cancellationToken);

            if (parent is null)
                return Result.NotFound("Parent product category was not found.");
        }

        var duplicateSlugExists = await repository.AnyAsync(
            new ProductCategoryByParentAndSlugSpec(request.ParentId, slug), cancellationToken);

        if (duplicateSlugExists)
            return Result.Conflict("Product category with the same parent and slug already exists.");

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
