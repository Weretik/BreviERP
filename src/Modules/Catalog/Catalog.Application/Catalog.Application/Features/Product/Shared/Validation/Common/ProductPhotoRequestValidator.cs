using Catalog.Application.Features.Product.Shared.Validation.Resources;
using Catalog.Application.Features.Product.Commands.CreateProduct;
using Catalog.Application.Features.Product.Commands.CreateProduct.Content;

namespace Catalog.Application.Features.Product.Shared.Validation.Common;

internal sealed class ProductPhotoRequestValidator : AbstractValidator<ProductPhotoRequest>
{
    public ProductPhotoRequestValidator()
    {
        RuleFor(x => x.MediaFileId)
            .GreaterThan(0)
            .WithMessage(_ => ProductValidationMessages.Positive("ID медіафайлу", "ID медиафайла"));

        RuleFor(x => x.Alt)
            .MaximumLength(500)
            .WithMessage(_ => ProductValidationMessages.MaximumLength("Alt-текст", "Alt-текст", 500));

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .WithMessage(_ => ProductValidationMessages.Localize(
                "Порядок фото не може бути від'ємним.",
                "Порядок фото не может быть отрицательным."));
    }
}
