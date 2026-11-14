using Catalog.Application.Features.Pricing.Shared.Ppe.Validation;
using Catalog.Application.Features.Pricing.Shared.Validation;

namespace Catalog.Application.Features.Pricing.Commands.CreatePpeRuleVersion;

public sealed class CreatePpeRuleVersionValidator : AbstractValidator<CreatePpeRuleVersionCommand>
{
    public CreatePpeRuleVersionValidator()
    {
        RuleFor(x => x.RequestId).ValidRequestId();
        RuleFor(x => x.ExpectedVersionId).NotEqual(Guid.Empty).WithMessage("Неправильна версія. Оновіть сторінку.");
        RuleFor(x => x.Actor).ValidActor();
        RuleFor(x => x.Values).Must(PpeVersionValidation.Rule)
            .WithMessage("Перевірте правило: код, джерело, дату й націнки (не від'ємні, не більше восьми знаків після коми).");
    }
}
