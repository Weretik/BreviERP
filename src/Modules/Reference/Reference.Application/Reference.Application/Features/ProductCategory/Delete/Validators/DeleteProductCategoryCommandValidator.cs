namespace Reference.Application.Features.ProductCategory.Delete.Validators;

public sealed class DeleteProductCategoryCommandValidator : AbstractValidator<DeleteProductCategoryCommand>
{
    public DeleteProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор категорії товарів має бути більшим за 0.");
    }
}
