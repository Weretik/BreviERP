using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.Shared.Validation.Content;

internal sealed class InformationBlockRequestValidator : AbstractValidator<InformationBlockRequest>
{
    public InformationBlockRequestValidator()
    {
        RuleFor(x => x.TitleUk)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required(ProductValidationFields.InformationBlockTitleUk))
            .MaximumLength(200)
            .WithMessage(_ => ProductValidationMessages.MaximumLength(ProductValidationFields.InformationBlockTitleUk, 200));

        RuleFor(x => x.TitleRu)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required(ProductValidationFields.InformationBlockTitleRu))
            .MaximumLength(200)
            .WithMessage(_ => ProductValidationMessages.MaximumLength(ProductValidationFields.InformationBlockTitleRu, 200));

        RuleFor(x => x.TextUk)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required(ProductValidationFields.InformationBlockTextUk))
            .MaximumLength(2_000)
            .WithMessage(_ => ProductValidationMessages.MaximumLength(ProductValidationFields.InformationBlockTextUk, 2_000));

        RuleFor(x => x.TextRu)
            .NotEmpty()
            .WithMessage(_ => ProductValidationMessages.Required(ProductValidationFields.InformationBlockTextRu))
            .MaximumLength(2_000)
            .WithMessage(_ => ProductValidationMessages.MaximumLength(ProductValidationFields.InformationBlockTextRu, 2_000));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_ => ProductValidationMessages.Localize(
                "Порядок не може бути від'ємним.",
                "Порядок не может быть отрицательным."));
    }
}
