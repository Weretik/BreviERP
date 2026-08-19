using Catalog.Application.Features.Product.Create.DTOs;

namespace Catalog.Application.Features.Product.Shared.Validation.Content;

internal sealed class CharacteristicRowRequestValidator : AbstractValidator<CharacteristicRowRequest>
{
    public CharacteristicRowRequestValidator()
    {
        RuleFor(x => x.LabelUk).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LabelRu).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ValueUk).NotEmpty().MaximumLength(1_000);
        RuleFor(x => x.ValueRu).NotEmpty().MaximumLength(1_000);
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
