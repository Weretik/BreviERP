namespace Catalog.Application.Features.Product.Update.Validators;

using Catalog.Application.Features.Product.Shared.Validation;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

public sealed class ReplaceProductCommandValidator : AbstractValidator<ReplaceProductCommand>
{
    public ReplaceProductCommandValidator()
    {
        RuleFor(x => x.Id).InclusiveBetween(1, 1_000_000_000).WithMessage(_ => ProductValidationMessages.Localize("ID товару має бути від 1 до 1 000 000 000.", "ID товара должен быть от 1 до 1 000 000 000."));
        RuleFor(x => x.Request).NotNull().WithMessage(_ => ProductValidationMessages.Required("Запит на оновлення", "Запрос на обновление"));
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Id).Equal(x => x.Id).WithMessage(_ => ProductValidationMessages.Localize("ID у запиті має збігатися з ID маршруту.", "ID в запросе должен совпадать с ID маршрута."));
            RuleFor(x => x.Request).SetValidator(new ProductRequestValidator());
        });
    }
}
