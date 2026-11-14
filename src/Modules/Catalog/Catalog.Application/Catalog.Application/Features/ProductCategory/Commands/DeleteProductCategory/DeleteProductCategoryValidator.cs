namespace Catalog.Application.Features.ProductCategory.Commands.DeleteProductCategory;

public sealed class DeleteProductCategoryValidator : AbstractValidator<DeleteProductCategoryCommand>
{
    public DeleteProductCategoryValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор категорії товарів має бути більшим за 0.");
    }
}


