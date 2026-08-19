using Catalog.Application.Features.Product.Create.DTOs;

namespace Catalog.Application.Features.Product.Shared.Validation.Content;

internal sealed class CharacteristicTableRequestValidator : AbstractValidator<CharacteristicTableRequest>
{
    public CharacteristicTableRequestValidator()
    {
        RuleFor(x => x.TitleUk).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TitleRu).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Rows).NotNull();
        When(x => x.Rows is not null, () => RuleForEach(x => x.Rows).SetValidator(new CharacteristicRowRequestValidator()));
        RuleFor(x => x.SortOrder).GreaterThanOrEqualTo(0);
    }
}
