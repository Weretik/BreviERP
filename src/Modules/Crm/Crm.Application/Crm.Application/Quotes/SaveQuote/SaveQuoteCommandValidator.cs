using Crm.Application.Quotes.Validation;

namespace Crm.Application.Quotes.SaveQuote;

public sealed class SaveQuoteCommandValidator : AbstractValidator<SaveQuoteCommand>
{
    public SaveQuoteCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteInputValidator());
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
    }
}
