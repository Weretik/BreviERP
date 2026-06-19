namespace Reference.Application.Features.ProductCategory.Update.Validators;

public sealed class UpdateProductCategoryCommandValidator : AbstractValidator<UpdateProductCategoryCommand>
{
    public UpdateProductCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Request).NotNull();

        RuleFor(x => x.Request.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.RuName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Request.Slug)
            .NotEmpty()
            .MaximumLength(160)
            .Matches("^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must contain lowercase letters, numbers, and hyphens only.");

        RuleFor(x => x.Request.ParentId)
            .GreaterThan(0)
            .When(x => x.Request.ParentId.HasValue);

        RuleFor(x => x.Request.SortOrder)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Request.Description)
            .MaximumLength(1000);
    }
}
