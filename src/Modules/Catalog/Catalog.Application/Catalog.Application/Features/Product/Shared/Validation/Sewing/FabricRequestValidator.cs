using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation.Resources;
namespace Catalog.Application.Features.Product.Shared.Validation.Sewing;

internal sealed class FabricRequestValidator : AbstractValidator<FabricRequest>
{
    public FabricRequestValidator()
    {
        RuleFor(x => x.FabricId)
            .GreaterThan(0)
            .WithMessage(_ => ProductValidationMessages
                .Positive("ID тканини", "ID ткани"));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_ => ProductValidationMessages.Localize(
                "Порядок тканини не може бути від'ємним.",
                "Порядок ткани не может быть отрицательным."));

    }
}
