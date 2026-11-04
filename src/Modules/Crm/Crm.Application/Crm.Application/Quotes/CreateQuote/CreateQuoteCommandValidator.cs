using Crm.Application.Quotes.Validation;

namespace Crm.Application.Quotes.CreateQuote;

public sealed class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteCommandValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteInputValidator());
    }
}
