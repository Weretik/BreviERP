using Crm.Application.Features.Quotes.Shared.Introductions.Validation;

namespace Crm.Application.Features.Quotes.Commands.SaveQuoteIntroduction;

public sealed class SaveQuoteIntroductionValidator : AbstractValidator<SaveQuoteIntroductionCommand>
{
    public SaveQuoteIntroductionValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteIntroductionInputValidator());
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
    }
}
