using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.Create.Validators;

public sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Request).NotNull().WithMessage(_ => ProductValidationMessages.Required("Запит", "Запрос"));
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id).InclusiveBetween(1, 1_000_000_000)
                .WithMessage(_ => ProductValidationMessages.Localize("ID товару має бути від 1 до 1 000 000 000.", "ID товара должен быть от 1 до 1 000 000 000."));
            RuleFor(x => x.Request).SetValidator(new ProductRequestValidator());
        });
    }
}
