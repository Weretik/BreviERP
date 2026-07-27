using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.ProductCategory.Shared.Specifications;
using Catalog.Domain.ProductCategories.ValueObjects;
using ProductCategoryEntity = Catalog.Domain.ProductCategories.Entities.ProductCategory;

namespace Catalog.Application.Features.ProductCategory.Create;

public sealed class CreateProductCategoryCommandHandler(ICatalogRepository<ProductCategoryEntity> repository)
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
                    "РљР°С‚РµРіРѕСЂС–СЏ С‚РѕРІР°СЂС–РІ Р· С‚Р°РєРёРј С–РґРµРЅС‚РёС„С–РєР°С‚РѕСЂРѕРј СѓР¶Рµ С–СЃРЅСѓС”."));
            }

            if (nameExists)
            {
                validationErrors.Add(new ValidationError(
                    "Request.Name",
                    "РљР°С‚РµРіРѕСЂС–СЏ С‚РѕРІР°СЂС–РІ Р· С‚Р°РєРѕСЋ РЅР°Р·РІРѕСЋ СѓР¶Рµ С–СЃРЅСѓС”."));
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
                    "РљР°С‚РµРіРѕСЂС–СЏ С‚РѕРІР°СЂС–РІ РЅРµ РјРѕР¶Рµ Р±СѓС‚Рё РІР»Р°СЃРЅРѕСЋ Р±Р°С‚СЊРєС–РІСЃСЊРєРѕСЋ РєР°С‚РµРіРѕСЂС–С”СЋ.")]);
            }

            parent = await repository.FirstOrDefaultAsync(
                new ProductCategoryByIdSpec(request.ParentId.Value), cancellationToken);

            if (parent is null)
            {
                return Result.Invalid([new ValidationError(
                    "Request.ParentId",
                    "Р‘Р°С‚СЊРєС–РІСЃСЊРєСѓ РєР°С‚РµРіРѕСЂС–СЋ С‚РѕРІР°СЂС–РІ РЅРµ Р·РЅР°Р№РґРµРЅРѕ.")]);
            }
        }

        var duplicateSlugExists = await repository.AnyAsync(
            new ProductCategoryByParentAndSlugSpec(request.ParentId, slug), cancellationToken);

        if (duplicateSlugExists)
        {
            return Result.Invalid([new ValidationError(
                "Request.Slug",
                "РљР°С‚РµРіРѕСЂС–СЏ С‚РѕРІР°СЂС–РІ Р· С‚Р°РєРёРј slug СѓР¶Рµ С–СЃРЅСѓС” РІ С†С–Р№ Р±Р°С‚СЊРєС–РІСЃСЊРєС–Р№ РєР°С‚РµРіРѕСЂС–С—.")]);
        }

        var entity = ProductCategoryEntity.Create(
            id,
            name,
            ruName,
            slug,
            request.ParentId.HasValue ? ProductCategoryId.Create(request.ParentId.Value) : null,
            parent?.Path,
            request.SortOrder,
            request.IsActive,
            request.Description);

        await repository.AddAsync(entity, cancellationToken);

        return Result.Success(entity.Id.Value);
    }
}


