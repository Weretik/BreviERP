using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation.Resources;
using Catalog.Domain.Products.ValueObjects;

namespace Catalog.Application.Features.Product.Shared.Validation.Ppe;

internal sealed class PercentRequestValidator : AbstractValidator<PercentRequest>
{
    public PercentRequestValidator()
    {
        RuleFor(x => x.Source)
            .IsInEnum()
            .WithMessage(_ => ProductValidationMessages.Invalid("Джерело відсотка", "Источник процента"));

        When(x => x.Source == PricePercentSource.Reference, () =>
        {
            RuleFor(x => x.AdditionalReferenceId)
                .NotNull()
                .WithMessage(_ => ProductValidationMessages.Required(
                    "ID довідникового відсотка",
                    "ID справочного процента"));
            RuleFor(x => x.AdditionalReferenceId)
                .Must(value => value is > 0)
                .WithMessage(_ => ProductValidationMessages.Positive(
                    "ID довідникового відсотка",
                    "ID справочного процента"));
            RuleFor(x => x.CustomPercent)
                .Null()
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "Ручний відсоток не дозволений для довідникового джерела.",
                    "Ручной процент не разрешён для справочного источника."));
        });

        When(x => x.Source == PricePercentSource.Custom, () =>
        {
            RuleFor(x => x.CustomPercent)
                .NotNull()
                .WithMessage(_ => ProductValidationMessages.Required("Ручний відсоток", "Ручной процент"));
            RuleFor(x => x.CustomPercent)
                .Must(value => value is >= 0)
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "Ручний відсоток не може бути від'ємним.",
                    "Ручной процент не может быть отрицательным."));
            RuleFor(x => x.AdditionalReferenceId)
                .Null()
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "ID довідникового відсотка не дозволений для ручного джерела.",
                    "ID справочного процента не разрешён для ручного источника."));
        });
    }
}
