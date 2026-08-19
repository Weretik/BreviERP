using Catalog.Application.Features.Product.Shared.Validation;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.Delete.Validators;

public sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id).InclusiveBetween(1, 1_000_000_000)
            .WithMessage(_ => ProductValidationMessages.Localize("ID товару має бути від 1 до 1 000 000 000.", "ID товара должен быть от 1 до 1 000 000 000."));
    }
}
