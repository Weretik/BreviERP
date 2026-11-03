using Reference.Application.Features.ProductSection.Shared.Validators;

namespace Reference.Application.Features.ProductSection.Create.Validators;

public sealed class CreateProductSectionCommandValidator : AbstractValidator<CreateProductSectionCommand>
{
    public CreateProductSectionCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення розділу не може бути порожнім.");

        When(x => x.Request is not null, () =>
            RuleFor(x => x.Request).SetValidator(new ProductSectionFieldsValidator()));
    }
}
