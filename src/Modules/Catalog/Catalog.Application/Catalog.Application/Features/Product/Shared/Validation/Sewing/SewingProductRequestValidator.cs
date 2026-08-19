using Catalog.Application.Features.Product.Create.DTOs;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.Shared.Validation.Sewing;

internal sealed class SewingProductRequestValidator : AbstractValidator<SewingProductRequest>
{
    public SewingProductRequestValidator()
    {
        RuleFor(x => x.MetersPerProduct)
            .GreaterThan(0)
            .WithMessage(_ => ProductValidationMessages.Positive("Метрів на виріб", "Метров на изделие"));

        RuleFor(x => x.Fabrics)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Тканини", "Ткани"));

        When(x => x.Fabrics is not null, () =>
        {
            RuleForEach(x => x.Fabrics).SetValidator(new FabricRequestValidator());
            RuleFor(x => x.Fabrics)
                .Must(items => items.Select(x => x.FabricId).Distinct().Count() == items.Count)
                .WithMessage(_ => ProductValidationMessages.Duplicate("Тканини", "Ткани"));
            RuleFor(x => x.Fabrics)
                .Must(items => items.Count(x => x.IsPrimary) <= 2)
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "Основних тканин може бути не більше двох.",
                    "Основных тканей может быть не больше двух."));
            RuleFor(x => x.Fabrics)
                .Must(items => items.Count != 1 || items.First().IsPrimary)
                .WithMessage(_ => ProductValidationMessages.Localize(
                    "Єдина тканина має бути основною.",
                    "Единственная ткань должна быть основной."));
        });

        RuleFor(x => x.Accessories)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Фурнітура", "Фурнитура"));

        When(x => x.Accessories is not null, () =>
        {
            RuleForEach(x => x.Accessories).SetValidator(new AccessoryRequestValidator());
            RuleFor(x => x.Accessories)
                .Must(items => items.Select(x => x.GarmentAccessoryId).Distinct().Count() == items.Count)
                .WithMessage(_ => ProductValidationMessages.Duplicate("Фурнітура", "Фурнитура"));
        });

        RuleFor(x => x.OperationIds)
            .NotNull()
            .WithMessage(_ => ProductValidationMessages.Required("Операції", "Операции"));

        When(x => x.OperationIds is not null, () =>
        {
            RuleForEach(x => x.OperationIds)
                .GreaterThan(0)
                .WithMessage(_ => ProductValidationMessages.Positive("ID операції", "ID операции"));
            RuleFor(x => x.OperationIds)
                .Must(ids => ids.Distinct().Count() == ids.Count)
                .WithMessage(_ => ProductValidationMessages.Duplicate("Операції", "Операции"));
        });
    }
}
