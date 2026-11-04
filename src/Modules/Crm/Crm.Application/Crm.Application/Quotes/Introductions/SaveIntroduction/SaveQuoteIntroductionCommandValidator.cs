using Crm.Application.Quotes.Introductions.Validation;

namespace Crm.Application.Quotes.Introductions.SaveIntroduction;

public sealed class SaveQuoteIntroductionCommandValidator : AbstractValidator<SaveQuoteIntroductionCommand>
{
    public SaveQuoteIntroductionCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteIntroductionInputValidator());
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
    }
}
