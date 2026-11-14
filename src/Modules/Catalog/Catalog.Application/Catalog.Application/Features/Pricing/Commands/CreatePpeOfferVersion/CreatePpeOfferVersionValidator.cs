using Catalog.Application.Features.Pricing.Shared.Ppe.Validation;
using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Commands.CreatePpeOfferVersion;

public sealed class CreatePpeOfferVersionValidator : AbstractValidator<CreatePpeOfferVersionCommand>
{
    public CreatePpeOfferVersionValidator()
    {
        RuleFor(x => x.ProductId).ValidProductId();
        RuleFor(x => x.RequestId).ValidRequestId();
        RuleFor(x => x.ExpectedVersionId).NotEqual(Guid.Empty).WithMessage("Неправильна версія. Оновіть сторінку.");
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Values).Must(PpeVersionValidation.Offer)
            .WithMessage("Перевірте пропозицію: джерело, дату й суми (не більше шести знаків після коми, собівартість більше 0).");
    }
}
