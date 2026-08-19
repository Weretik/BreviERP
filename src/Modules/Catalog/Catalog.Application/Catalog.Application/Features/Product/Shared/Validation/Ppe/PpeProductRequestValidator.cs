using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.Shared.Validation.Ppe;

internal sealed class PpeProductRequestValidator : AbstractValidator<PpeProductRequest>
{
    public PpeProductRequestValidator()
    {
        RuleFor(x => x.SupplierId)
            .GreaterThan(0)
            .WithMessage(_ => ProductValidationMessages.Positive("ID постачальника", "ID поставщика"));

        RuleFor(x => x.BasePrice)
            .GreaterThan(0)
            .WithMessage(_ => ProductValidationMessages.Positive("Базова ціна", "Базовая цена"));

        RuleFor(x => x.RetailPercent)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Роздрібний відсоток", "Розничный процент"));

        RuleFor(x => x.WholesalePercent)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Оптовий відсоток", "Оптовый процент"));

        When(x => x.RetailPercent is not null, () =>
            RuleFor(x => x.RetailPercent!).SetValidator(new PercentRequestValidator()));

        When(x => x.WholesalePercent is not null, () =>
            RuleFor(x => x.WholesalePercent!).SetValidator(new PercentRequestValidator()));
    }
}
