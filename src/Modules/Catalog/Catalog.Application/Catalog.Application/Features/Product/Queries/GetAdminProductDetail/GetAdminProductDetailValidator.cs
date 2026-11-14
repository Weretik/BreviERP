using Catalog.Application.Features.Product.Shared.Validation;
using Catalog.Application.Features.Product.Shared.Validation.Resources;

namespace Catalog.Application.Features.Product.Queries.GetAdminProductDetail;

public sealed class GetAdminProductDetailValidator : AbstractValidator<GetAdminProductDetailQuery>
{
    public GetAdminProductDetailValidator()
    {
        RuleFor(x => x.Id).InclusiveBetween(1, 1_000_000_000)
            .WithMessage(_ => ProductValidationMessages.Localize("ID товару має бути від 1 до 1 000 000 000.", "ID товара должен быть от 1 до 1 000 000 000."));
    }
}
