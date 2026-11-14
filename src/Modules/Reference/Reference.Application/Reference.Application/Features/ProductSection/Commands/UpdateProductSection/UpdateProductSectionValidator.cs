using Reference.Application.Features.ProductSection.Shared;

namespace Reference.Application.Features.ProductSection.Commands.UpdateProductSection;

public sealed class UpdateProductSectionValidator : AbstractValidator<UpdateProductSectionCommand>
{
    public UpdateProductSectionValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Ідентифікатор розділу має бути більшим за 0.");

        RuleFor(x => x.Request)
            .NotNull()
            .WithMessage("Запит на зміну розділу не може бути порожнім.");

        When(x => x.Request is not null, () =>
            RuleFor(x => x.Request).SetValidator(new ProductSectionFieldsValidator()));
    }
}
