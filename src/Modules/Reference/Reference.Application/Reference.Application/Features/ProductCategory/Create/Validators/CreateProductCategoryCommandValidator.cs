namespace Reference.Application.Features.ProductCategory.Create.Validators;

public sealed class CreateProductCategoryCommandValidator : AbstractValidator<CreateProductCategoryCommand>
{
    public CreateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення категорії товарів не може бути порожнім.");

        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор категорії товарів має бути більшим за 0.");

            RuleFor(x => x.Request.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва категорії товарів є обов'язковою.")
                .MaximumLength(200)
                .WithMessage("Назва категорії товарів не може перевищувати 200 символів.");

            RuleFor(x => x.Request.RuName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Назва категорії товарів російською є обов'язковою.")
                .MaximumLength(200)
                .WithMessage("Назва категорії товарів російською не може перевищувати 200 символів.");

            RuleFor(x => x.Request.Slug)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Slug категорії є обов'язковим.")
                .MaximumLength(160)
                .WithMessage("Slug категорії не може перевищувати 160 символів.")
                .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
                .WithMessage("Slug категорії може містити лише малі латинські літери, цифри та дефіси.");

            RuleFor(x => x.Request.ParentId)
                .GreaterThan(0)
                .WithMessage("Ідентифікатор батьківської категорії має бути більшим за 0.")
                .When(x => x.Request.ParentId.HasValue);

            RuleFor(x => x.Request.SortOrder)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Порядок сортування не може бути від'ємним.");

            RuleFor(x => x.Request.Description)
                .MaximumLength(1000)
                .WithMessage("Опис категорії товарів не може перевищувати 1000 символів.")
                .When(x => !string.IsNullOrWhiteSpace(x.Request.Description));
        });
    }
}
