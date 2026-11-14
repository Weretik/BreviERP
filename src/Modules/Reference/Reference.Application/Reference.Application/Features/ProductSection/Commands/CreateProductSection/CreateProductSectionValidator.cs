using Reference.Application.Features.ProductSection.Shared;

namespace Reference.Application.Features.ProductSection.Commands.CreateProductSection;

public sealed class CreateProductSectionValidator : AbstractValidator<CreateProductSectionCommand>
{
    public CreateProductSectionValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на створення розділу не може бути порожнім.");

        When(x => x.Request is not null, () =>
            RuleFor(x => x.Request).SetValidator(new ProductSectionFieldsValidator()));
    }
}
