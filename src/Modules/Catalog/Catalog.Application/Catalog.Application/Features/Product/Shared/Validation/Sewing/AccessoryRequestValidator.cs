using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.Shared.Validation.Sewing;

internal sealed class AccessoryRequestValidator : AbstractValidator<AccessoryRequest>
{
    public AccessoryRequestValidator()
    {
        RuleFor(x => x.GarmentAccessoryId)
            .GreaterThan(0)
            .WithMessage(_ => ProductValidationMessages.Positive("ID фурнітури", "ID фурнитуры"));

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage(_ => ProductValidationMessages.Positive("Кількість фурнітури", "Количество фурнитуры"));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_ => ProductValidationMessages.Localize(
                "Порядок фурнітури не може бути від'ємним.",
                "Порядок фурнитуры не может быть отрицательным."));
    }
}
