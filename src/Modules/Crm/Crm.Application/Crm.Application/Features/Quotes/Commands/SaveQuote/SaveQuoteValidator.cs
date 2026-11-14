using Crm.Application.Features.Quotes.Shared.Validation;

namespace Crm.Application.Features.Quotes.Commands.SaveQuote;

public sealed class SaveQuoteValidator : AbstractValidator<SaveQuoteCommand>
{
    public SaveQuoteValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteInputValidator());
        RuleFor(x => x.Input.Version).GreaterThan(0).When(x => x.Input is not null);
    }
}
