using Crm.Application.Features.Quotes.Shared.Validation;

namespace Crm.Application.Features.Quotes.Commands.CreateQuote;

public sealed class CreateQuoteValidator : AbstractValidator<CreateQuoteCommand>
{
    public CreateQuoteValidator()
    {
        RuleFor(x => x.Actor).NotNull();
        RuleFor(x => x.Actor.UserId).GreaterThan(0).When(x => x.Actor is not null);
        RuleFor(x => x.Input).NotNull().SetValidator(new SaveQuoteInputValidator());
    }
}
